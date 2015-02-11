using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Crystalbyte.Paranoia.Themes {
    [Export(typeof(Theme))]
    public sealed class LightTheme : Theme {
        public override string GetName() {
            return "Light";
        }

        public override ResourceDictionary GetThemeResources() {
            var url = string.Format(Pack.Relative, typeof(LightTheme).Assembly.FullName, "/Themes.Light.Resources.xaml");
            return (ResourceDictionary)Application.LoadComponent(new Uri(url, UriKind.Relative));
        }
    }
}
