#region Using directives

using Crystalbyte.Paranoia.Messaging;
using System;

#endregion

namespace Crystalbyte.Paranoia {
    public static class StringExtensions {

        public static bool ContainsIgnoreCase(this string text, string value) {
            return text.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        public static SecurityPolicy ToSecurityPolicy(this string text) {
            if (text.ContainsIgnoreCase("ssl")) {
                return SecurityPolicy.Implicit;
            }
            return text.ContainsIgnoreCase("tls") ? SecurityPolicy.Explicit : SecurityPolicy.None;
        }
    }
}