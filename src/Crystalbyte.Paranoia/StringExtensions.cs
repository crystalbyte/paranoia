﻿#region Using directives

using System;
using Crystalbyte.Paranoia.Mail;

#endregion

namespace Crystalbyte.Paranoia {
    public static class StringExtensions {
        public static bool ContainsIgnoreCase(this string text, string value) {
            return text.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) > -1;
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
                }
                else if (shifted < minChar) {
                    shifted += maxChar;
                }

                buffer[i] = Convert.ToChar(shifted);
            }

            return new string(buffer);
        }
    }
}