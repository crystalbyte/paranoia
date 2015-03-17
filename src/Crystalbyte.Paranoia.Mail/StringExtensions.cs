#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia.Mail
// 
// Crystalbyte.Paranoia.Mail is free software: you can redistribute it and/or modify
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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    internal static class StringExtensions {
        public static string ToCommaSeparatedValues<T>(this IEnumerable<T> items) {
            return items.DefaultIfEmpty()
                .Select(x => x.ToString())
                .Aggregate((c, n) => c + "," + n);
        }

        public static bool ContainsIgnoreCase(this string text, string value) {
            return text.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        public static string TrimNil(this string value) {
            if (string.IsNullOrWhiteSpace(value)) {
                return string.Empty;
            }

            return value.IndexOf("NIL", StringComparison.InvariantCultureIgnoreCase) > -1
                ? string.Empty
                : value;
        }

        public static bool IsNumeric(this string value) {
            double id;
            return double.TryParse(value, out id);
        }

        public static string TrimQuotes(this string value) {
            return value.Trim(new[] {'"'});
        }

        public static string RemoveComments(this string input) {
            return Regex.Replace(input, @"\(.+\)", string.Empty);
        }

        public static string TrimAny(this string value) {
            return value.TrimAny(1);
        }

        public static string TrimAny(this string value, int count) {
            var length = value.Length - count*2;
            return value.Substring(count, length);
        }

        public static string ToBlockText(this string text, int lineLength) {
            var breaks = (text.Length/lineLength);
            if (breaks == 0) {
                return text;
            }
            using (var reader = new StringReader(text)) {
                using (var writer = new StringWriter()) {
                    for (var i = 0; i < breaks; i++) {
                        var buffer = new char[lineLength];
                        reader.Read(buffer, 0, lineLength);
                        writer.Write(buffer);
                        writer.Write(Environment.NewLine);
                    }
                    writer.WriteLine(reader.ReadToEnd());
                    return writer.ToString();
                }
            }
        }
    }
}