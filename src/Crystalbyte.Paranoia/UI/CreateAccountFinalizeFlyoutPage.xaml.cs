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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Win32;
using NLog;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for CreateAccountFinalizeFlyoutPage.xaml
    /// </summary>
    public partial class CreateAccountFinalizeFlyoutPage : INavigationAware {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public CreateAccountFinalizeFlyoutPage() {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(FlyoutCommands.Continue, OnContinue));
            CommandBindings.Add(new CommandBinding(FlyoutCommands.Cancel, OnClose));
            CommandBindings.Add(new CommandBinding(MailboxCommands.Browse, OnBrowseMailboxes));
            CommandBindings.Add(new CommandBinding(MailboxCommands.SelectRole, OnSelectRole, OnCanSelectRole));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnCloseMailboxSelection));
            CommandBindings.Add(new CommandBinding(SignatureCommands.SelectFile, OnSelectFile));
        }

        #endregion

        private void OnSelectFile(object sender, ExecutedRoutedEventArgs e) {
            var dialog = new OpenFileDialog();
            var result = dialog.ShowDialog();
            if ((result.HasValue && !result.Value) || !result.HasValue) {
                return;
            }

            var context = (MailAccountContext)DataContext;
            context.SignaturePath = dialog.FileNames.First();
        }

        private void OnCanSelectRole(object sender, CanExecuteRoutedEventArgs e) {
            if (DataContext == null) {
                e.CanExecute = false;
                return;
            }

            var account = (MailAccountContext)DataContext;
            var mailbox = account.Mailboxes.FirstOrDefault(x => x.IsSelectedSubtly);
            e.CanExecute = mailbox != null && mailbox.IsSelectable;
        }

        private Popup GetPopupByParameter(string param) {
            switch (param) {
                case MailboxRoles.Sent:
                    return SentMailboxSelectionPopup;
                case MailboxRoles.Trash:
                    return TrashMailboxSelectionPopup;
                case MailboxRoles.Junk:
                    return JunkMailboxSelectionPopup;
                case MailboxRoles.Draft:
                    return DraftMailboxSelectionPopup;
            }

            throw new ArgumentOutOfRangeException(param);
        }

        private void OnCloseMailboxSelection(object sender, ExecutedRoutedEventArgs e) {
            var param = e.Parameter as string;
            if (string.IsNullOrEmpty(param)) {
                throw new ArgumentNullException();
            }

            var popup = GetPopupByParameter(param);
            popup.IsOpen = false;
        }

        private void OnSelectRole(object sender, ExecutedRoutedEventArgs e) {
            var param = e.Parameter as string;
            if (string.IsNullOrEmpty(param)) {
                throw new ArgumentNullException();
            }

            var popup = GetPopupByParameter(param);
            popup.IsOpen = false;

            var account = (MailAccountContext)DataContext;
            var mailbox = account.Mailboxes.FirstOrDefault(x => x.IsSelectedSubtly);
            if (mailbox != null) {
                SetMailboxRoleByParam(mailbox, param);
            }
        }

        private void SetMailboxRoleByParam(MailboxContext mailbox, string param) {
            var account = (MailAccountContext)DataContext;
            switch (param) {
                case MailboxRoles.Sent:
                    account.SentMailboxName = mailbox.Name;
                    break;
                case MailboxRoles.Trash:
                    account.TrashMailboxName = mailbox.Name;
                    break;
                case MailboxRoles.Junk:
                    account.JunkMailboxName = mailbox.Name;
                    break;
                case MailboxRoles.Draft:
                    account.DraftMailboxName = mailbox.Name;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(param);
            }
        }

        private void OnBrowseMailboxes(object sender, ExecutedRoutedEventArgs e) {
            var param = e.Parameter as string;
            if (string.IsNullOrEmpty(param)) {
                throw new ArgumentNullException();
            }

            ShowMailboxSelection(param);
        }

        private void ShowMailboxSelection(string param) {
            var account = (MailAccountContext)DataContext;
            account.Mailboxes.ForEach(x => x.IsSelectedSubtly = false);

            var mailbox = GetMailboxByParameter(param);
            if (mailbox != null) {
                mailbox.IsSelectedSubtly = true;
            }

            var popup = GetPopupByParameter(param);
            popup.IsOpen = true;
        }

        private MailboxContext GetMailboxByParameter(string param) {
            var account = (MailAccountContext)DataContext;
            switch (param) {
                case MailboxRoles.Sent:
                    return account.GetSentMailbox();
                case MailboxRoles.Trash:
                    return account.GetTrashMailbox();
                case MailboxRoles.Junk:
                    return account.GetJunkMailbox();
                case MailboxRoles.Draft:
                    return account.GetDraftMailbox();
            }

            throw new ArgumentOutOfRangeException(param);
        }

        private void OnFlyoutClosed(object sender, EventArgs e) {
            var account = (MailAccountContext)DataContext;
            account.Testing = null;

            App.Context.FlyoutClosed -= OnFlyoutClosed;
        }

        private static void OnClose(object sender, ExecutedRoutedEventArgs e) {
            try {
                App.Context.CloseFlyout();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private async void OnContinue(object sender, ExecutedRoutedEventArgs e) {
            try {
                ContinueButton.IsEnabled = false;
                App.Context.CloseFlyout();

                var account = (MailAccountContext)DataContext;
                await account.SaveAsync();

            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                ContinueButton.IsEnabled = true;
            }
        }

        private void OnStoreCopyRadioButtonChecked(object sender, RoutedEventArgs e) {
            var account = (MailAccountContext)DataContext;
            account.StoreCopiesOfSentMessages = true;
        }

        private void OnDontStoreCopyRadioButtonChecked(object sender, RoutedEventArgs e) {
            // Why is it being called when navigating away with empty DataContext ?
            if (DataContext == null) {
                return;
            }

            var account = (MailAccountContext)DataContext;
            account.StoreCopiesOfSentMessages = false;
        }

        private void OnAnyTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            CommandManager.InvalidateRequerySuggested();
        }

        #region Implementation of INavigationAware

        public async void OnNavigated(NavigationEventArgs e) {
            DataContext = NavigationArguments.Pop();
            var account = (MailAccountContext)DataContext;

            StoreCopyRadioButton.IsChecked = account.StoreCopiesOfSentMessages;
            DontStoreCopyRadioButton.IsChecked = !account.StoreCopiesOfSentMessages;

            App.Context.FlyoutClosed += OnFlyoutClosed;

            try {
                await account.SyncMailboxesAsync();
            } catch (Exception ex) {
                // TODO: Display error to the user.
                Debug.WriteLine(ex);
            }
        }

        public void OnNavigating(NavigatingCancelEventArgs e) {
            var account = (MailAccountContext)DataContext;
            switch (e.NavigationMode) {
                case NavigationMode.Back:
                    NavigationArguments.Push(account);
                    break;
            }
        }

        #endregion
    }
}