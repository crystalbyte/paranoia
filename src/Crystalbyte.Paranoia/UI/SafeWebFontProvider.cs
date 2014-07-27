using System.Collections.Generic;
using System.Windows.Media;

namespace Crystalbyte.Paranoia.UI {
    public sealed class SafeWebFonts : List<FontFamily> {
        public SafeWebFonts() {
            AddRange(new[] {
                new FontFamily("Georgia"),
                new FontFamily("Palatino Linotype"),
                new FontFamily("Times New Roman"),
                new FontFamily("Arial"),
                new FontFamily("Arial Black"),
                new FontFamily("Comic Sans MS"),
                new FontFamily("Impact"),
                new FontFamily("Lucida Sans Unicode"),
                new FontFamily("Tahoma"),
                new FontFamily("Trebuchet MS"),
                new FontFamily("Verdana"),
                new FontFamily("Courier New"),
                new FontFamily("Lucida Console")
            });
        }
    }
}
