using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Windows;

namespace Crystalbyte.Paranoia.Themes {
    [Export(typeof(Theme))]
    public sealed class LightTheme : Theme {
        public override string GetName() {
            return "Light";
        }

        public override IEnumerable<ResourceDictionary> GetThemeResources() {
            var resources = new[] {
                "/Themes.Light.Resources.xaml",
                "/Themes.Light.Button.xaml",
                "/Themes.Light.CheckBox.xaml",
                "/Themes.Light.ComboBox.xaml",
                "/Themes.Light.ContextMenu.xaml",
                "/Themes.Light.GridSplitter.xaml",
                "/Themes.Light.ListView.xaml",
                "/Themes.Light.PasswordBox.xaml",
                "/Themes.Light.ProgressBar.xaml",
                "/Themes.Light.RadioButton.xaml",
                "/Themes.Light.RichTextBox.xaml",
                "/Themes.Light.ScrollViewer.xaml",
                "/Themes.Light.Slider.xaml",
                "/Themes.Light.StatusBar.xaml",
                "/Themes.Light.TextBlock.xaml",
                "/Themes.Light.TextBox.xaml",
                "/Themes.Light.ToggleButton.xaml",
                "/Themes.Light.Tooltip.xaml",
                "/Themes.Light.TreeView.xaml",
                "/Themes.Light.MetroButton.xaml"
            };

            return resources
                .Select(x => string.Format(Pack.Relative, typeof(LightTheme).Assembly.FullName, x))
                .Select(url => (ResourceDictionary)Application.LoadComponent(new Uri(url, UriKind.Relative)));
        }
    }
}
