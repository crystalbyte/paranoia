using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using Crystalbyte.Paranoia.Properties;

namespace Crystalbyte.Paranoia.Data {
    internal sealed class SQLiteModelAnalyzer {
        private readonly Type _type;

        public SQLiteModelAnalyzer(Type type) {
            _type = type;
        }

        public string GetTableCreateScript() {
            var tableName = _type.GetCustomAttribute<TableAttribute>().Name;
            var properties = _type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var keyExists = properties.Any(x => x.GetCustomAttribute<KeyAttribute>() != null);
            if (!keyExists) {
                var message = string.Format(Resources.MissingKeyTemplate, _type.Name);
                throw new Exception(message);
            }

            var foreignKeyProperty = properties.FirstOrDefault(x => x.GetCustomAttribute<ForeignKeyAttribute>() != null);

            using (var writer = new StringWriter()) {
                writer.Write("CREATE TABLE ");
                writer.Write(tableName);
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

            var isPrimaryKey = info.GetCustomAttribute<KeyAttribute>() != null;

            using (var writer = new StringWriter()) {
                writer.Write(name);
                writer.Write(" ");
                writer.Write(type);
                if (!isPrimaryKey)
                    return writer.ToString();

                writer.Write(" ");
                writer.Write("PRIMARY KEY");
                return writer.ToString();
            }
        }

        private static string CreateForeignKeyDefinition(MemberInfo info) {

            var foreignKey = info.GetCustomAttribute<ForeignKeyAttribute>();
            var masterTypeName = foreignKey.Name;

            var declaringType = info.DeclaringType;
            if (declaringType == null) {
                throw new NullReferenceException(Resources.DeclaringTypeNull);
            }

            var masterProperty = declaringType.GetProperty(masterTypeName);
            var masterType = masterProperty.PropertyType;
            var masterKey =
                masterType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .First(x => x.GetCustomAttribute<KeyAttribute>() != null);

            var masterTable = masterType.GetCustomAttribute<TableAttribute>();
            var masterTableName = masterTable == null ? masterType.Name : masterTable.Name;

            string name;
            var hasColumnAttribute = TryReadColumnAttribute(info, out name);
            if (!hasColumnAttribute) {
                name = info.Name;
            }

            using (var writer = new StringWriter()) {
                writer.Write("FOREIGN KEY");
                writer.Write("(");
                writer.Write(name);
                writer.Write(") ");
                writer.Write("REFERENCES ");
                writer.Write(masterTableName);
                writer.Write("(");
                writer.Write(masterKey.Name);
                writer.Write(")");
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
