using System;
using System.Windows.Media;

namespace Crystalbyte.Paranoia.UI {
    public static class ColorExtensions {
        public static string ToHex(this Color color, bool alphaChannel) {
            return String.Format("#{0}{1}{2}{3}",
                                 alphaChannel ? color.A.ToString("X2") : String.Empty,
                                 color.R.ToString("X2"),
                                 color.G.ToString("X2"),
                                 color.B.ToString("X2"));
        }
    }
}
