#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using Directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Documents;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia.Data.SQLite {
    internal sealed class ModelAnalyzer {
        private readonly Type _type;

        public ModelAnalyzer(Type type) {
            _type = type;
        }

        public bool TryGetIndexCreateScripts(out List<string> scripts) {
            var tableName = _type.GetCustomAttribute<TableAttribute>().Name;

            var properties = _type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var indexProperties = properties.Where(x => x.GetCustomAttribute<IndexAttribute>() != null).ToArray();
            if (indexProperties.Length == 0) {
                scripts = null;
                return false;
            }

            scripts = new List<string>();

            foreach (var index in indexProperties.Select(x => {
                var attribute = x.GetCustomAttribute<ColumnAttribute>();
                return attribute != null
                    ? attribute.Name
                    : x.Name;
            })) {
                using (var writer = new StringWriter()) {
                    writer.Write("CREATE INDEX {0}_{1}_index ON {0}(", tableName, index);
                    writer.Write(index);
                    writer.Write(");");
                    scripts.Add(writer.ToString());
                }
            }

            return true;
        }

        public string GetTableCreateScript() {
            var tableName = _type.GetCustomAttribute<TableAttribute>().Name;
            var properties = _type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var keyExists = properties.Any(x => x.GetCustomAttribute<KeyAttribute>() != null);
            if (!keyExists) {
                var message = string.Format(Resources.MissingKeyTemplate, _type.Name);
                throw new SQLiteException(message);
            }

            var foreignKeyProperty = properties.FirstOrDefault(x => x.GetCustomAttribute<ForeignKeyAttribute>() != null);

            string moduleName = null;
            var vAttribute = _type.GetCustomAttribute<VirtualAttribute>();
            if (vAttribute != null) {
                moduleName = vAttribute.GetModuleName();
            }

            var isVirtual = !string.IsNullOrEmpty(moduleName);
            using (var writer = new StringWriter()) {
                writer.Write(!isVirtual ? "CREATE TABLE {0}" : "CREATE VIRTUAL TABLE {0}", tableName);
                if (isVirtual) {
                    writer.Write(" USING {0}", moduleName);
                }

                writer.Write("(");
                writer.Write(string.Join(", ", properties
                    .Where(x => !x.PropertyType.IsGenericType)
                    .Where(x => x.PropertyType.GetCustomAttribute<TableAttribute>() == null)
                    .Select(CreateSQLiteColumnDefinition)));

                if (foreignKeyProperty != null) {
                    writer.Write(", ");
                    writer.Write(CreateForeignKeyDefinition(foreignKeyProperty));
                }

                writer.Write(");");

                return writer.ToString();
            }
        }

        private static string CreateSQLiteColumnDefinition(PropertyInfo info) {
            var type = info.PropertyType.ToSQLiteType();

            string name;
            var hasColumnAttribute = TryReadColumnAttribute(info, out name);
            if (!hasColumnAttribute) {
                name = info.Name;
            }

            CollatingSequence sequence;
            var isPrimaryKey = info.GetCustomAttribute<KeyAttribute>() != null;
            var hasCollate = TryReadCollateAttribute(info, out sequence);
            var hasTimestamp = TryReadTimestampAttribute(info);

            using (var writer = new StringWriter()) {
                writer.Write(name);
                writer.Write(" ");
                writer.Write(type);
                if (hasCollate) {
                    switch (sequence) {
                        case CollatingSequence.Binary:
                            writer.Write(" COLLATE BINARY");
                            break;
                        case CollatingSequence.NoCase:
                            writer.Write(" COLLATE NOCASE");
                            break;
                        case CollatingSequence.RTrim:
                            writer.Write(" COLLATE RTRIM");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (hasTimestamp) {
                    writer.Write(" DEFAULT CURRENT_TIMESTAMP");
                }

                if (!isPrimaryKey)
                    return writer.ToString();

                writer.Write(" ");
                writer.Write("PRIMARY KEY");
                return writer.ToString();
            }
        }

        private static bool TryReadTimestampAttribute(MemberInfo info) {
            var attribute = info.GetCustomAttribute<DefaultAttribute>();
            return attribute != null && attribute.Function == DatabaseFunction.CurrentTimestamp;
        }

        private static bool TryReadCollateAttribute(MemberInfo info, out CollatingSequence sequence) {
            sequence = CollatingSequence.Binary;
            var attribute = info.GetCustomAttribute<CollateAttribute>();
            if (attribute == null) {
                return false;
            }

            sequence = attribute.Sequence;
            return true;
        }

        private static string CreateForeignKeyDefinition(PropertyInfo info) {
            var foreignKeyAttribute = info.GetCustomAttribute<ForeignKeyAttribute>();
            var foreignKeyPropertyName = foreignKeyAttribute.Name;

            var declaringType = info.DeclaringType;
            if (declaringType == null) {
                throw new NullReferenceException(Resources.DeclaringTypeNull);
            }

            var foreignKeyProperty = declaringType.GetProperty(foreignKeyPropertyName);

            var masterType = info.PropertyType;
            if (masterType == null) {
                throw new NullReferenceException(Resources.PropertyTypeNull);
            }

            var masterKeyProperty = masterType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .First(x => x.GetCustomAttribute<KeyAttribute>() != null);

            var masterTable = masterType.GetCustomAttribute<TableAttribute>();
            var masterTableName = masterTable == null ? masterType.Name : masterTable.Name;

            string foreignKeyName;
            var hasColumnAttribute = TryReadColumnAttribute(foreignKeyProperty, out foreignKeyName);
            if (!hasColumnAttribute) {
                foreignKeyName = foreignKeyProperty.Name;
            }

            string masterKeyName;
            hasColumnAttribute = TryReadColumnAttribute(masterKeyProperty, out masterKeyName);
            if (!hasColumnAttribute) {
                masterKeyName = masterKeyProperty.Name;
            }

            using (var writer = new StringWriter()) {
                writer.Write("FOREIGN KEY");
                writer.Write("(");
                writer.Write(foreignKeyName);
                writer.Write(") ");
                writer.Write("REFERENCES ");
                writer.Write(masterTableName);
                writer.Write("(");
                writer.Write(masterKeyName);
                writer.Write(") ON DELETE CASCADE");
                return writer.ToString();
            }
        }

        private static bool TryReadColumnAttribute(MemberInfo info, out string name) {
            var attribute = info.GetCustomAttribute<ColumnAttribute>();
            if (attribute == null) {
                name = string.Empty;
                return false;
            }

            name = attribute.Name;
            return true;
        }
    }
}