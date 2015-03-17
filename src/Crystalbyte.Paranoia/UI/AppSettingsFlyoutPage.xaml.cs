#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using Directives

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.Themes;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for AppSettingsFlyoutPage.xaml
    /// </summary>
    public partial class AppSettingsFlyoutPage : INavigationAware {
        #region Construction

        public AppSettingsFlyoutPage() {
            InitializeComponent();
            DataContext = App.Context;

            CommandBindings.Add(new CommandBinding(FlyoutCommands.Cancel, OnCancel));
            Loaded += (sender, e) => Focus();
        }

        private static void OnCancel(object sender, ExecutedRoutedEventArgs e) {
            App.Context.CloseFlyout();
        }

        #endregion

        public void OnNavigated(NavigationEventArgs e) {
            ThemeComboBox.SelectedValue = App.Context.Themes
                .FirstOrDefault(x => string.Compare(x.Name,
                    Settings.Default.Theme, StringComparison.InvariantCultureIgnoreCase) == 0);

            var color = ColorConverter.ConvertFromString(Settings.Default.Accent);
            if (color != null) {
                AccentListView.SelectedValue = color;
            }
        }

        public void OnNavigating(NavigatingCancelEventArgs e) {
            // Nada
        }

        private void OnThemeSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var view = (ComboBox) sender;
            var theme = (Theme) view.SelectedValue;
            if (string.Compare(Settings.Default.Theme,
                theme.Name, StringComparison.InvariantCultureIgnoreCase) != 0) {
                ((App) Application.Current).ApplyTheme(theme);
            }
        }

        private void OnAccentSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var view = (ListView) sender;
            var color = (Color) view.SelectedItem;
            if (string.Compare(Settings.Default.Accent,
                color.ToHex(false), StringComparison.InvariantCultureIgnoreCase) != 0) {
                ((App) Application.Current).ApplyAccent(color);
            }
        }
    }
}