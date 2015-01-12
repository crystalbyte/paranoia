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

        public override IEnumerable<ResourceDictionary> GetThemeResources() {
            var resources = new[] {
                "/Themes.Dark.Resources.xaml",
                "/Themes.Dark.GroupBox.xaml",
                "/Themes.Dark.ScrollViewer.xaml",
                "/Themes.Dark.TabControl.xaml",
                "/Themes.Dark.Button.xaml",
                "/Themes.Dark.RepeatButton.xaml",
                "/Themes.Dark.CheckBox.xaml",
                "/Themes.Dark.ComboBox.xaml",
                "/Themes.Dark.Menu.xaml",
                "/Themes.Dark.GridSplitter.xaml",
                "/Themes.Dark.ListView.xaml",
                "/Themes.Dark.PasswordBox.xaml",
                "/Themes.Dark.RadioButton.xaml",
                "/Themes.Dark.Slider.xaml",
                "/Themes.Dark.StatusBar.xaml",
                "/Themes.Dark.TextBlock.xaml",
                "/Themes.Dark.TextBox.xaml",
                "/Themes.Dark.ToggleButton.xaml",
                "/Themes.Dark.Tooltip.xaml",
                "/Themes.Dark.TreeView.xaml",
                "/Themes.Dark.SuggestiveTextBox.xaml",
                "/Themes.Dark.WatermarkTextBox.xaml",
                "/Themes.Dark.MetroProgressBar.xaml",
                "/Themes.Dark.MetroProgressRing.xaml",
                "/Themes.Dark.MetroButton.xaml"
            };

            return resources
                .Select(x => string.Format(Pack.Relative, typeof(DarkTheme).Assembly.FullName, x))
                .Select(url => (ResourceDictionary)Application.LoadComponent(new Uri(url, UriKind.Relative)));
        }
    }
}
