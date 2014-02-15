#region Using directives

using Crystalbyte.Paranoia.Contexts;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///   Interaction logic for CreateAccountScreen.xaml
    /// </summary>
    public partial class IdentityCreationFrame {
        public IdentityCreationFrame() {
            InitializeComponent();
        }

        private void OnFrameDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            TransferDataContext();
            RestartPageNavigation();
        }

        private void RestartPageNavigation() {
            while (NavigationFrame.CanGoBack) {
                NavigationFrame.RemoveBackEntry();
            }

            var url = string.Format("/UI/{0}.xaml", typeof(IdentityCreationPage).Name);
            NavigationFrame.Navigate(new Uri(url, UriKind.Relative));
        }

        private void TransferDataContext() {
            var page = NavigationFrame.Content as Page;
            if (page == null) {
                return;
            }

            page.DataContext = DataContext;
        }

        private void OnFrameNavigated(object sender, NavigationEventArgs e) {
            TransferDataContext();
        }
    }
}