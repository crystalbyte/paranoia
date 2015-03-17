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
using System.Collections.Generic;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia.Data {
    internal static class TypeExtensions {
        public static string ToSQLiteType(this Type type) {
            const string text = "TEXT";
            const string integer = "INTEGER";
            const string blob = "BLOB";
            const string real = "REAL";

            if (type.IsEnum) {
                return integer;
            }

            var types = new Dictionary<Type, string>
            {
                {typeof (Int32), integer},
                {typeof (Int64), integer},
                {typeof (Int16), integer},
                {typeof (byte), integer},
                {typeof (bool), integer},
                {typeof (string), text},
                {typeof (char), integer},
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