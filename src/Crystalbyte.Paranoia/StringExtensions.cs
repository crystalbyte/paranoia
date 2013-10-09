#region Using directives

using System;

#endregion

namespace Crystalbyte.Paranoia {
    public static class StringExtensions {
        public static bool ContainsIgnoreCase(this string text, string value) {
            return text.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) > -1;
        }
    }
}