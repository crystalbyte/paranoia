#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Crystalbyte.Paranoia.Mail;

#endregion

namespace Crystalbyte.Paranoia {
    public static class StringExtensions {
        public static bool ContainsIgnoreCase(this string text, string value) {
            return text.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        public static bool EqualsIgnoreCase(this string text, string value) {
            return string.Compare(text, value, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static SecurityProtocol ToSecurityPolicy(this string text) {
            if (text.ContainsIgnoreCase("ssl")) {
                return SecurityProtocol.Implicit;
            }
            return text.ContainsIgnoreCase("tls") ? SecurityProtocol.Explicit : SecurityProtocol.None;
        }

        public static string Caesar(this string value, int shift) {
            if (string.IsNullOrWhiteSpace(value)) {
                return string.Empty;
            }

            var maxChar = Convert.ToInt32(char.MaxValue);
            var minChar = Convert.ToInt32(char.MinValue);

            var buffer = value.ToCharArray();

            for (var i = 0; i < buffer.Length; i++) {
                var shifted = Convert.ToInt32(buffer[i]) + shift;

                if (shifted > maxChar) {
                    shifted -= maxChar;
                } else if (shifted < minChar) {
                    shifted += maxChar;
                }

                buffer[i] = Convert.ToChar(shifted);
            }

            return new string(buffer);
        }

        internal static Dictionary<String, String> ToPageArguments(this string s) {
            const string pattern = "[A-Za-z0-9%\\.-]+=[A-Za-z0-9%\\.-]+";
            var matches = Regex.Matches(s, pattern, RegexOptions.IgnoreCase);

            var pairs = (from Match match in matches
                         select match.Value.Split('='));

            var dictionary = new Dictionary<string, string>();
            foreach (var pair in pairs) {
                var key = pair[0];
                var value = pair[1];
                if (dictionary.ContainsKey(key)) {
                    dictionary[key] = value;
                    continue;
                }

                dictionary.Add(key, value);
            }

            return dictionary;
        }
    }
}