using System;
using System.Composition;
using System.Windows;

namespace Crystalbyte.Paranoia.Themes {
    [Export(typeof(Theme))]
    public sealed class LightTheme : Theme {
        public override string GetName() {
            return "Light";
        }

        public override ResourceDictionary GetThemeResources() {
            var url = string.Format(Pack.Relative, typeof(LightTheme).Assembly.FullName, "/ThemeResources.xaml");
            var resource = (ResourceDictionary)Application.LoadComponent(new Uri(url, UriKind.Relative));
            return resource;
        }
    }
}
