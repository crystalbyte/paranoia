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

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for CreateAccountStartFlyoutPage.xaml
    /// </summary>
    public partial class CreateAccountStartFlyoutPage : INavigationAware {
        public CreateAccountStartFlyoutPage() {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(FlyoutCommands.Cancel, OnCancel));
            CommandBindings.Add(new CommandBinding(FlyoutCommands.Continue, OnContinue));
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e) {
            var box = (PasswordBox) sender;
            var context = (MailAccountContext) DataContext;
            context.ImapPassword = box.Password;
            context.SmtpPassword = box.Password;
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

            var account = (MailAccountContext) DataContext;
            if (account.IsAutoDetectPreferred) {
                ContinueButton.IsEnabled = false;
                await DetectSettingsAsync();
            }

            var uri = typeof (CreateAccountServerSettingsFlyoutPage).ToPageUri();
            service.Navigate(uri);
        }

        private async Task DetectSettingsAsync() {
            var account = (MailAccountContext) DataContext;
            await account.DetectSettingsAsync();
        }

        private static void OnCancel(object sender, ExecutedRoutedEventArgs e) {
            App.Context.CloseFlyout();
        }

        private void OnAddressTextChanged(object sender, TextChangedEventArgs e) {
            CheckForContinuation();
        }

        private void OnNameTextChanged(object sender, TextChangedEventArgs e) {
            CheckForContinuation();
        }

        #region Implementation of INavigationAware

        public void OnNavigated(NavigationEventArgs e) {
            DataContext = NavigationArguments.Pop();
            CheckForContinuation();

            PasswordBox.PasswordChanged += OnPasswordChanged;
            NameTextBox.Focus();
        }

        public void OnNavigating(NavigatingCancelEventArgs e) {
            var account = (MailAccountContext) DataContext;
            switch (e.NavigationMode) {
                case NavigationMode.New:
                    PasswordBox.PasswordChanged -= OnPasswordChanged;
                    NavigationArguments.Push(account);
                    break;
            }
        }

        #endregion
    }
}