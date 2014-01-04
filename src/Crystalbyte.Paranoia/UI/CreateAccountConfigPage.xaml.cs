#region Using directives

using System.Windows;
using System.Windows.Input;
using Crystalbyte.Paranoia.Contexts;
using System.Windows.Controls;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///   Interaction logic for CreateAccountConfigPage.xaml
    /// </summary>
    public partial class CreateAccountConfigPage {
        public CreateAccountConfigPage() {
            ScreenContext = App.AppContext.CreateAccountScreenContext;
            InitializeComponent();
        }

        public CreateAccountScreenContext ScreenContext { 
            get { return DataContext as CreateAccountScreenContext; } 
            set { DataContext = value; } 
        }

        private void OnPortPreviewTextInput(object sender, TextCompositionEventArgs e) {
            if (!char.IsDigit(e.Text, e.Text.Length - 1)) {
                e.Handled = true;
            }
        }

        private void OnImapPasswordChanged(object sender, RoutedEventArgs e) {
            var box = sender as PasswordBox;
            if (box == null) {
                return;
            }

            if (string.IsNullOrWhiteSpace(box.Password)) {
                return;
            }

            ScreenContext.ImapPassword = box.Password;
        }

        private void OnSmtpPasswordChanged(object sender, RoutedEventArgs e) {
            var box = sender as PasswordBox;
            if (box == null) {
                return;
            }

            if (string.IsNullOrWhiteSpace(box.Password)) {
                return;
            }

            ScreenContext.SmtpPassword = box.Password;
        }

        private void OnImapPasswordBoxLoaded(object sender, RoutedEventArgs e) {
            var box = sender as PasswordBox;
            if (box == null) {
                return;
            }
         

            box.Password = ScreenContext.ImapPassword;
        }

        private void OnSmtpPasswordBoxLoaded(object sender, RoutedEventArgs e) {
            var box = sender as PasswordBox;
            if (box == null) {
                return;
            }


            box.Password = ScreenContext.ImapPassword;
        }
    }
}