#region Using directives

using System;
using System.Windows;

#endregion

namespace Crystalbyte.Paranoia.UI.Pages {
    /// <summary>
    ///     Interaction logic for CreateKeyPage.xaml
    /// </summary>
    public partial class UnlockKeyPage {
        public UnlockKeyPage() {
            InitializeComponent();
            DataContext = new UnlockKeyPairContext();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            PasswordBox.Focus();
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e) {
            AcceptButton.IsEnabled = !String.IsNullOrEmpty(PasswordBox.Password);
        }

        private void KrakenButtonClick(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("http://www.quickmeme.com/img/11/11ddc80a462f07b608d1e290461c8837c2c79019d280b3b73443c15959c8e3c6.jpg");
            KrakenButton.IsEnabled = false;
        }
    }
}