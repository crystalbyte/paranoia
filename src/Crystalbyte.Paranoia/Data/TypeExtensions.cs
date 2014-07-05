using System;
using System.Collections.Generic;
using Crystalbyte.Paranoia.Properties;

namespace Crystalbyte.Paranoia.Data {
    internal static class TypeExtensions {
        public static string ToSQLiteType(this Type type) {

            const string text = "TEXT";
            const string integer = "INTEGER";
            const string blob = "BLOB";
            const string real = "REAL";

            var types = new Dictionary<Type, string> {
                {typeof (Int32), integer},
                {typeof (Int64), integer},
                {typeof (Int16), integer},
                {typeof (byte), integer},
                {typeof (bool), integer},
                {typeof (string), text},
                {typeof (char), text},
                {typeof (DateTime), text},
                {typeof (byte[]), blob},
                {typeof (float), real},
                {typeof (double), real}
            };

            if (types.ContainsKey(type))
                return types[type];

            var message = string.Format(Resources.TypeNotSupportedTemplate, type.Name);
            throw new NotSupportedException(message);
        }
    }
}
