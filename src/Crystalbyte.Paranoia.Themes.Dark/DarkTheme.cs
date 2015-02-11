using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Windows;

namespace Crystalbyte.Paranoia.Themes {
    [Export(typeof(Theme))]
    public sealed class DarkTheme : Theme {
        public override string GetName() {
            return "Dark";
        }

        public override ResourceDictionary GetThemeResources() {
            var url = string.Format(Pack.Relative, typeof(DarkTheme).Assembly.FullName, "/Themes.Dark.Resources.xaml");
            return (ResourceDictionary)Application.LoadComponent(new Uri(url, UriKind.Relative));
        }
    }
}
