#region Using directives

using System;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public static class TypeExtensions {
        public static Uri ToPageUri(this Type type) {
            return new Uri(string.Format("/UI/{0}.xaml", type.Name), UriKind.RelativeOrAbsolute);
        }

        public static Uri ToPageUri(this Type type, string arguments) {
            return new Uri(string.Format("/UI/{0}.xaml{1}", type.Name, arguments), UriKind.Relative);
        }
    }
}