using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using Crystalbyte.Paranoia.Properties;

namespace Crystalbyte.Paranoia.Data {
    internal static class DbDataReaderExtension {

        public static IEnumerable<T> Read<T>(this DbDataReader reader) where T : class, new() {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var propertyMap = new Dictionary<string, PropertyInfo>();
            propertyMap.AddRange(properties.Select(x => {
                var attribute = x.GetCustomAttribute<ColumnAttribute>();
                if (attribute != null) {
                    return new KeyValuePair<string, PropertyInfo>(
                        attribute.Name, x);
                }
                return new KeyValuePair<string, PropertyInfo>(
                    x.Name, x);
            }));

            var values = new List<T>();
            while (reader.Read()) {
                var container = Activator.CreateInstance<T>();
                foreach (var ordinal in Enumerable.Range(0, reader.FieldCount)) {
                    var name = reader.GetName(ordinal);

                    PropertyInfo info;
                    var success = propertyMap.TryGetValue(name, out info);
                    if (!success) {
                        var message = string.Format(Resources.ColumnNotFoundTemplate, name, typeof(T).FullName);
                        throw new SQLiteException(message);
                    }

                    var value = reader.GetValue(ordinal);
                    info.SetValue(container, Convert.ChangeType(value, info.PropertyType));
                }
                values.Add(container);
            }

            return values;
        }
    }
}
