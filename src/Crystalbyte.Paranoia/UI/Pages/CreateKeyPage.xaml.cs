using System;
using System.Windows;

namespace Crystalbyte.Paranoia.UI.Pages {
    /// <summary>
    /// Interaction logic for CreateKeyPage.xaml
    /// </summary>
    public partial class CreateKeyPage {

        public CreateKeyPage() {
            DataContext = new CreateKeyPairContext();
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            CheckForContinuation();
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e) {
            CheckForContinuation();
        }

        private void OnConfirmPasswordChanged(object sender, RoutedEventArgs e) {
            CheckForContinuation();
        }

        private void OnRiskStatementChecked(object sender, RoutedEventArgs e) {
            CheckForContinuation();
        }

        private void CheckForContinuation() {
            AcceptButton.IsEnabled = RiskCheckBox.IsChecked != null
                                     && RiskCheckBox.IsChecked.Value
                                     && !string.IsNullOrEmpty(PasswordBox.Password)
                                     && !string.IsNullOrEmpty(ConfirmPasswordBox.Password)
                                     && String.Compare(PasswordBox.Password, ConfirmPasswordBox.Password, StringComparison.InvariantCulture) == 0;
        }

        private void OnRiskStatementUnchecked(object sender, RoutedEventArgs e) {
            CheckForContinuation();
        }
    }
}
