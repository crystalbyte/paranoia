#region Using directives

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Crystalbyte.Paranoia.Mail;
using NavigationCommands = Crystalbyte.Paranoia.UI.FlyoutCommands;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for CreateAccountServerSettingsFlyoutPage.xaml
    /// </summary>
    public partial class CreateAccountServerSettingsFlyoutPage : INavigationAware {

        public CreateAccountServerSettingsFlyoutPage() {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(NavigationCommands.Continue, OnContinue));
            CommandBindings.Add(new CommandBinding(NavigationCommands.Cancel, OnClose));
        }

        private static void OnClose(object sender, ExecutedRoutedEventArgs e) {
            App.Context.CloseFlyout();
        }

        private void OnContinue(object sender, ExecutedRoutedEventArgs e) {
            var service = NavigationService;
            if (service == null) {
                throw new NullReferenceException("NavigationService");
            }

            var uri = typeof(CreateAccountFinalizeFlyoutPage).ToPageUri();
            service.Navigate(uri);
        }

        private void OnImapSecurityProtocolSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var account = (MailAccountContext)DataContext;
            account.ImapPort = (short)(account.ImapSecurity == SecurityProtocol.Implicit ? 993 : 143);
        }

        private void OnImapPasswordChanged(object sender, RoutedEventArgs e) {
            var box = (PasswordBox)sender;
            var account = (MailAccountContext)DataContext;
            account.ImapPassword = box.Password;
        }

        private void OnSmtpPasswordChanged(object sender, RoutedEventArgs e) {
            var box = (PasswordBox)sender;
            var account = (MailAccountContext)DataContext;
            account.SmtpPassword = box.Password;
        }

        private void OnUseImapCredentialsChecked(object sender, RoutedEventArgs e) {
            var account = (MailAccountContext)DataContext;
            var button = ((RadioButton)sender);
            if (button.IsChecked != null) {
                account.UseImapCredentialsForSmtp = button.IsChecked.Value;
            }
        }

        private void OnUseSmtpCredentialsChecked(object sender, RoutedEventArgs e) {
            var account = (MailAccountContext)DataContext;
            var button = ((RadioButton)sender);
            if (button.IsChecked != null) {
                account.UseImapCredentialsForSmtp = !button.IsChecked.Value;
            }
        }

        private void OnFlyoutClosing(object sender, EventArgs e) {
            SmtpPasswordBox.PasswordChanged -= OnSmtpPasswordChanged;
            ImapPasswordBox.PasswordChanged -= OnImapPasswordChanged;
        }

        private void OnFlyoutClosed(object sender, EventArgs e) {
            var account = (MailAccountContext)DataContext;
            account.Testing = null;

            App.Context.FlyoutClosed -= OnFlyoutClosed;
        }

        private void SetFocus() {
            var account = (MailAccountContext)DataContext;
            if (!string.IsNullOrEmpty(account.ImapHost)) {
                ImapPasswordBox.Focus();
            } else {
                NameTextBox.Focus();
            }
        }

        private void HookUpChangeEvents() {
            var account = (MailAccountContext)DataContext;

            SmtpPasswordBox.Password = account.SmtpPassword;
            SmtpPasswordBox.PasswordChanged += OnSmtpPasswordChanged;

            ImapPasswordBox.Password = account.ImapPassword;
            ImapPasswordBox.PasswordChanged += OnImapPasswordChanged;

            UseImapCredentialsRadioButton.IsChecked = account.UseImapCredentialsForSmtp;
            UseSmtpCredentialsRadioButton.IsChecked = !account.UseImapCredentialsForSmtp;

            App.Context.FlyoutClosing += OnFlyoutClosing;
            App.Context.FlyoutClosed += OnFlyoutClosed;
        }

        #region Implementation of INavigationAware

        public void OnNavigated(NavigationEventArgs e) {
            DataContext = NavigationArguments.Pop();

            HookUpChangeEvents();
            SetFocus();
        }

        public void OnNavigating(NavigatingCancelEventArgs e) {
            var account = (MailAccountContext)DataContext;
            switch (e.NavigationMode) {
                case NavigationMode.New:
                case NavigationMode.Back:
                    SmtpPasswordBox.PasswordChanged -= OnSmtpPasswordChanged;
                    ImapPasswordBox.PasswordChanged -= OnImapPasswordChanged;
                    NavigationArguments.Push(account);
                    break;
            }
        }

        #endregion
    }
}