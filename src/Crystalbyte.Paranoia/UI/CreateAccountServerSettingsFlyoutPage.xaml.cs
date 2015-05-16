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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Crystalbyte.Paranoia.Mail;
using NLog;
using NavigationCommands = Crystalbyte.Paranoia.UI.FlyoutCommands;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for CreateAccountServerSettingsFlyoutPage.xaml
    /// </summary>
    public partial class CreateAccountServerSettingsFlyoutPage : INavigationAware {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CreateAccountServerSettingsFlyoutPage() {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(NavigationCommands.Continue, OnContinue));
            CommandBindings.Add(new CommandBinding(NavigationCommands.Cancel, OnClose));
        }

        private static void OnClose(object sender, ExecutedRoutedEventArgs e) {
            App.Context.CloseFlyout();
        }

        private async void OnContinue(object sender, ExecutedRoutedEventArgs e) {
            var service = NavigationService;
            if (service == null) {
                throw new NullReferenceException("NavigationService");
            }

            try {
                var account = (MailAccountContext) DataContext;
                throw new NotImplementedException("save account");
                await App.Context.PublishAccountAsync(account);
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }

            var uri = typeof (CreateAccountFinalizeFlyoutPage).ToPageUri();
            service.Navigate(uri);
        }

        private void OnSmtpSecurityProtocolSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var account = (MailAccountContext) DataContext;
            account.SmtpPort = (short) (account.SmtpSecurity == SecurityProtocol.Implicit ? 587 : 25);
        }

        private void OnImapSecurityProtocolSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var account = (MailAccountContext) DataContext;
            account.ImapPort = (short) (account.ImapSecurity == SecurityProtocol.Implicit ? 993 : 143);
        }

        private void OnImapPasswordChanged(object sender, RoutedEventArgs e) {
            var box = (PasswordBox) sender;
            var account = (MailAccountContext) DataContext;
            account.ImapPassword = box.Password;
        }

        private void OnSmtpPasswordChanged(object sender, RoutedEventArgs e) {
            var box = (PasswordBox) sender;
            var account = (MailAccountContext) DataContext;
            account.SmtpPassword = box.Password;
        }

        private void OnUseImapCredentialsChecked(object sender, RoutedEventArgs e) {
            var account = (MailAccountContext) DataContext;
            var button = ((RadioButton) sender);
            if (button.IsChecked != null) {
                account.UseImapCredentialsForSmtp = button.IsChecked.Value;
            }
        }

        private void OnUseSmtpCredentialsChecked(object sender, RoutedEventArgs e) {
            var account = (MailAccountContext) DataContext;
            var button = ((RadioButton) sender);
            if (button.IsChecked != null) {
                account.UseImapCredentialsForSmtp = !button.IsChecked.Value;
            }
        }

        private void OnFlyoutClosing(object sender, EventArgs e) {
            SmtpPasswordBox.PasswordChanged -= OnSmtpPasswordChanged;
            ImapPasswordBox.PasswordChanged -= OnImapPasswordChanged;
        }

        private void OnFlyoutClosed(object sender, EventArgs e) {
            var account = (MailAccountContext) DataContext;
            account.Testing = null;

            App.Context.FlyoutClosed -= OnFlyoutClosed;
        }

        private void SetFocus() {
            var account = (MailAccountContext) DataContext;
            if (!string.IsNullOrEmpty(account.ImapHost)) {
                ImapPasswordBox.Focus();
            }
            else {
                NameTextBox.Focus();
            }
        }

        private void HookUpChangeEvents() {
            var account = (MailAccountContext) DataContext;

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
            var account = (MailAccountContext) DataContext;
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