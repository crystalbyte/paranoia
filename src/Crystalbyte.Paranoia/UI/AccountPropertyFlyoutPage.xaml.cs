#region Using directives

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Navigation;
using Crystalbyte.Paranoia.Mail;
using Microsoft.Win32;
using NLog;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for AccountPropertyFlyoutPage.xaml
    /// </summary>
    public partial class AccountPropertyFlyoutPage : INavigationAware, ICancelationAware {

        #region Private Fields

        private RevisionTracker<MailAccountContext> _tracker;
        private readonly static Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public AccountPropertyFlyoutPage() {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(FlyoutCommands.Cancel, OnCancel));
            CommandBindings.Add(new CommandBinding(FlyoutCommands.Accept, OnAccept));
            CommandBindings.Add(new CommandBinding(MailboxCommands.Browse, OnBrowseMailboxes));
            CommandBindings.Add(new CommandBinding(MailboxCommands.SelectRole, OnSelectRole, OnCanSelectRole));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnCloseMailboxSelection));
            CommandBindings.Add(new CommandBinding(SignatureCommands.SelectFile, OnSelectFile));
        }

        #endregion

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

        private void OnCancel(object sender, ExecutedRoutedEventArgs e) {
            RevertChanges();
            App.Context.CloseFlyout();
        }

        private void RevertChanges() {
            _tracker.Stop();
            _tracker.Revert();
        }

        private async void OnAccept(object sender, ExecutedRoutedEventArgs e) {
            await SaveChanges();
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

        private void OnUseSmtpCredentialsChecked(object sender, RoutedEventArgs e) {
            var account = (MailAccountContext)DataContext;
            var button = ((RadioButton)sender);
            if (button.IsChecked != null) {
                account.UseImapCredentialsForSmtp = !button.IsChecked.Value;
            }
        }

        private async Task SaveChanges() {
            try {
                ContinueButton.IsEnabled = false;
                var account = (MailAccountContext)DataContext;
                await account.SaveAsync();
            } catch (Exception ex) {
                Logger.Error(ex);
            } finally {
                ContinueButton.IsEnabled = true;
                App.Context.CloseFlyout();
            }
        }

        private void OnFlyoutClosed(object sender, EventArgs e) {
            var account = (MailAccountContext)DataContext;
            account.Testing = null;

            App.Context.FlyoutClosed -= OnFlyoutClosed;
        }

        private void OnAnyTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            CommandManager.InvalidateRequerySuggested();
        }

        private void OnFlyoutClosing(object sender, EventArgs e) {
            SmtpPasswordBox.PasswordChanged -= OnSmtpPasswordChanged;
            ImapPasswordBox.PasswordChanged -= OnImapPasswordChanged;
        }

        private void HookUpChangeEvents() {
            var account = (MailAccountContext)DataContext;

            SmtpPasswordBox.Password = account.SmtpPassword;
            SmtpPasswordBox.PasswordChanged += OnSmtpPasswordChanged;

            ImapPasswordBox.Password = account.ImapPassword;
            ImapPasswordBox.PasswordChanged += OnImapPasswordChanged;

            UseImapCredentialsRadioButton.IsChecked = account.UseImapCredentialsForSmtp;
            UseSmtpCredentialsRadioButton.IsChecked = !account.UseImapCredentialsForSmtp;

            StoreCopyRadioButton.IsChecked = account.StoreCopiesOfSentMessages;
            DontStoreCopyRadioButton.IsChecked = !account.StoreCopiesOfSentMessages;

            App.Context.FlyoutClosing += OnFlyoutClosing;
            App.Context.FlyoutClosed += OnFlyoutClosed;
        }

        private void StartTracking() {
            var account = (MailAccountContext)DataContext;
            _tracker = new RevisionTracker<MailAccountContext>(account);
            _tracker.WithProperty(x => x.Name)
                    .WithProperty(x => x.Address)
                    .WithProperty(x => x.ImapHost)
                    .WithProperty(x => x.ImapPort)
                    .WithProperty(x => x.ImapUsername)
                    .WithProperty(x => x.ImapPassword)
                    .WithProperty(x => x.ImapSecurity)
                    .WithProperty(x => x.UseImapCredentialsForSmtp)
                    .WithProperty(x => x.SmtpHost)
                    .WithProperty(x => x.SmtpPort)
                    .WithProperty(x => x.SmtpUsername)
                    .WithProperty(x => x.SmtpPassword)
                    .WithProperty(x => x.SmtpSecurity)
                    .WithProperty(x => x.TrashMailboxName)
                    .WithProperty(x => x.JunkMailboxName)
                    .WithProperty(x => x.SentMailboxName)
                    .WithProperty(x => x.DraftMailboxName)
                    .WithProperty(x => x.StoreCopiesOfSentMessages);
            _tracker.Start();
        }

        #region Implementation of INavigationAware

        public void OnNavigated(NavigationEventArgs e) {
            DataContext = NavigationArguments.Pop();
            HookUpChangeEvents();
            StartTracking();
        }

        public void OnNavigating(NavigatingCancelEventArgs e) {
            // Nada
        }

        #endregion

        #region Implementation of ICancelationAware

        public void OnCanceled() {
            RevertChanges();
        }

        #endregion
    }
}