#region Using directives

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.Themes;
using NLog;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for AppSettingsFlyoutPage.xaml
    /// </summary>
    public partial class AppSettingsFlyoutPage : INavigationAware {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public AppSettingsFlyoutPage() {
            InitializeComponent();
            DataContext = App.Context;

            CommandBindings.Add(new CommandBinding(FlyoutCommands.Cancel, OnCancel));
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
            var view = (ComboBox)sender;
            var theme = (Theme)view.SelectedValue;
            if (string.Compare(Settings.Default.Theme,
                theme.Name, StringComparison.InvariantCultureIgnoreCase) != 0) {
                ((App)Application.Current).ChangeTheme(theme);
            }
        }

        private void OnAccentSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var view = (ListView)sender;
            var color = (Color)view.SelectedItem;
            if (string.Compare(Settings.Default.Accent,
                color.ToHex(false), StringComparison.InvariantCultureIgnoreCase) != 0) {
                ((App)Application.Current).ChangeAccent(color);
            }
        }
    }
}