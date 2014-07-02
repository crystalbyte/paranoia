#region Using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    internal static class StringExtensions {
        public static string ToCommaSeparatedValues<T>(this IEnumerable<T> items) {
            return items.Select(x => x.ToString()).Aggregate((c, n) => c + "," + n);
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