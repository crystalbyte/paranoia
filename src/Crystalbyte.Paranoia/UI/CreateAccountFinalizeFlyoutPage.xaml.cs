#region Using directives

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Win32;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for CreateAccountFinalizeFlyoutPage.xaml
    /// </summary>
    public partial class CreateAccountFinalizeFlyoutPage : INavigationAware {

        public CreateAccountFinalizeFlyoutPage() {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(NavigationCommands.Continue, OnContinue));
            CommandBindings.Add(new CommandBinding(NavigationCommands.Close, OnClose));
            CommandBindings.Add(new CommandBinding(MailboxSelectionCommands.Select, OnSelect));
            CommandBindings.Add(new CommandBinding(MailboxSelectionCommands.Commit, OnCommit, OnCanCommit));
            CommandBindings.Add(new CommandBinding(MailboxSelectionCommands.Cancel, OnCancel));
            CommandBindings.Add(new CommandBinding(SignatureCommands.SelectFile, OnSelectFile));
        }

        private void OnSelectFile(object sender, ExecutedRoutedEventArgs e) {
            var dialog = new OpenFileDialog();
            var result = dialog.ShowDialog();
            if ((result.HasValue && !result.Value) || !result.HasValue) {
                return;
            }

            var context = (MailAccountContext)DataContext;
            context.SignaturePath = dialog.FileNames.First();
        }

        private void OnCanCommit(object sender, CanExecuteRoutedEventArgs e) {
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

        private void OnCancel(object sender, ExecutedRoutedEventArgs e) {
            var param = e.Parameter as string;
            if (string.IsNullOrEmpty(param)) {
                throw new ArgumentNullException();
            }

            var popup = GetPopupByParameter(param);
            popup.IsOpen = false;
        }

        private void OnCommit(object sender, ExecutedRoutedEventArgs e) {
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

        private void OnSelect(object sender, ExecutedRoutedEventArgs e) {
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

        private void OnFlyOutClosed(object sender, EventArgs e) {
            var account = (MailAccountContext)DataContext;
            account.Testing = null;

            App.Context.FlyOutClosed -= OnFlyOutClosed;
        }

        private static void OnClose(object sender, ExecutedRoutedEventArgs e) {
            App.Context.CloseFlyOut();
        }

        private async void OnContinue(object sender, ExecutedRoutedEventArgs e) {
            var account = (MailAccountContext)DataContext;
            await SaveChangesAsync(account);
            App.Context.CloseFlyOut();
        }

        private static async Task SaveChangesAsync(MailAccountContext account) {
            await account.UpdateAsync();
        }

        public void OnNavigated(NavigationEventArgs e) {
            var account = (MailAccountContext)NavigationArguments.Pop();
            DataContext = account;

            StoreCopyRadioButton.IsChecked = account.StoreCopiesOfSentMessages;
            DontStoreCopyRadioButton.IsChecked = !account.StoreCopiesOfSentMessages;

            App.Context.FlyOutClosed += OnFlyOutClosed;
        }

        private void OnStoreCopyRadioButtonChecked(object sender, RoutedEventArgs e) {
            var account = (MailAccountContext)DataContext;
            account.StoreCopiesOfSentMessages = true;
        }

        private void OnDontStoreCopyRadioButtonChecked(object sender, RoutedEventArgs e) {
            var account = (MailAccountContext)DataContext;
            account.StoreCopiesOfSentMessages = false;
        }

        private void OnAnyTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}