#region Using directives

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Crystalbyte.Paranoia.Data;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for CreateAccountPage.xaml
    /// </summary>
    public partial class CreateAccountPage {
        public CreateAccountPage() {
            InitializeComponent();
            DataContext = new MailAccountContext(new MailAccountModel());

            CommandBindings.Add(new CommandBinding(NavigationCommands.Close, OnCancel));
            CommandBindings.Add(new CommandBinding(NavigationCommands.Continue, OnContinue));

            PasswordBox.PasswordChanged += OnPasswordChanged;
            Loaded += OnLoaded;
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e) {
            var box = (PasswordBox) sender;
            var context = (MailAccountContext) DataContext;
            context.ImapPassword = box.Password;
            context.SmtpPassword = box.Password;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            NameTextBox.Focus();
            CheckForContinuation();
        }

        private void OnAutoMagicButtonCheckedChanged(object sender, RoutedEventArgs e) {
            CheckForContinuation();
        }

        private void CheckForContinuation() {
            var context = (MailAccountContext) DataContext;

            ContinueButton.IsEnabled =
                (AutoMagicButton.IsChecked != null && !AutoMagicButton.IsChecked.Value)
                || (!string.IsNullOrEmpty(context.Name) && !string.IsNullOrEmpty(context.Address));
        }

        private async void OnContinue(object sender, ExecutedRoutedEventArgs e) {
            var service = NavigationService;
            if (service == null) {
                throw new NullReferenceException("NavigationService");
            }

            var context = (MailAccountContext) DataContext;
            if (context.IsAutoDetectPreferred) {
                ContinueButton.IsEnabled = false;
                await DetectSettingsAsync();
            }

            PasswordBox.PasswordChanged -= OnPasswordChanged;
            App.Context.TransitAccount = context;
            var uri = typeof (AccountPage).ToPageUri("?mode=new");
            service.Navigate(uri);
        }

        private async Task DetectSettingsAsync() {
            var context = (MailAccountContext) DataContext;
            await context.DetectSettingsAsync();
        }

        private static void OnCancel(object sender, ExecutedRoutedEventArgs e) {
            App.Context.CloseFlyOut();
        }

        private void OnAddressTextChanged(object sender, TextChangedEventArgs e) {
            CheckForContinuation();
        }

        private void OnNameTextChanged(object sender, TextChangedEventArgs e) {
            CheckForContinuation();
        }
    }
}