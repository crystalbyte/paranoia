#region Using directives

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Crystalbyte.Paranoia.Mail;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for CreateAccountServerSettingsFlyoutPage.xaml
    /// </summary>
    public partial class CreateAccountServerSettingsFlyoutPage : INavigationAware {
        private bool _discardOnClose;
        private RevisionTracker<MailAccountContext> _tracker;

        public CreateAccountServerSettingsFlyoutPage() {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(NavigationCommands.Continue, OnContinue));
            CommandBindings.Add(new CommandBinding(NavigationCommands.Close, OnClose));
            CommandBindings.Add(new CommandBinding(MailboxSelectionCommands.Cancel, OnCancel));

            Loaded += OnLoaded;
        }
 
 private void OnCancel(object sender, ExecutedRoutedEventArgs e) {
            var param = e.Parameter as string;
            if (string.IsNullOrEmpty(param)) {
                throw new ArgumentNullException();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            var context = (MailAccountContext)DataContext;
            if (!string.IsNullOrEmpty(context.ImapHost)) {
                ImapPasswordBox.Focus();
            } else {
                NameTextBox.Focus();
            }
        }

        private void OnFlyOutClosed(object sender, EventArgs e) {
            var account = (MailAccountContext)DataContext;
            account.Testing = null;

            App.Context.FlyOutClosed -= OnFlyOutClosed;
            if (_discardOnClose) {
                DiscardChanged();
            }
        }

        private static void OnClose(object sender, ExecutedRoutedEventArgs e) {
            App.Context.CloseFlyOut();
        }

        private void DiscardChanged() {
            _tracker.Stop();
            _tracker.Revert();
        }

        private async void OnContinue(object sender, ExecutedRoutedEventArgs e) {
            var account = (MailAccountContext)DataContext;
            await SaveChangesAsync(account);
            App.Context.CloseFlyOut();
        }

        private async Task SaveChangesAsync(MailAccountContext account) {
            _discardOnClose = false;
            _tracker.Stop();
            await account.UpdateAsync();
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

        public void OnNavigated(NavigationEventArgs e) {
            var account = (MailAccountContext) NavigationStore.Pop(GetType());

            _discardOnClose = true;
            _tracker = new RevisionTracker<MailAccountContext>(account)
                .WithProperty(x => x.Name)
                .WithProperty(x => x.Address)
                .WithProperty(x => x.ImapHost)
                .WithProperty(x => x.ImapPort)
                .WithProperty(x => x.ImapUsername)
                .WithProperty(x => x.ImapPassword)
                .WithProperty(x => x.ImapSecurity)
                .WithProperty(x => x.SmtpHost)
                .WithProperty(x => x.SmtpPort)
                .WithProperty(x => x.SmtpUsername)
                .WithProperty(x => x.SmtpPassword)
                .WithProperty(x => x.SmtpSecurity)
                .WithProperty(x => x.SentMailboxName)
                .WithProperty(x => x.DraftMailboxName)
                .WithProperty(x => x.TrashMailboxName)
                .WithProperty(x => x.JunkMailboxName)
                .WithProperty(x => x.SignaturePath)
                .WithProperty(x => x.UseImapCredentialsForSmtp)
                .Start();

            DataContext = account;

            SmtpPasswordBox.Password = account.SmtpPassword;
            SmtpPasswordBox.PasswordChanged += OnSmtpPasswordChanged;

            ImapPasswordBox.Password = account.ImapPassword;
            ImapPasswordBox.PasswordChanged += OnImapPasswordChanged;

            UseImapCredentialsRadioButton.IsChecked = account.UseImapCredentialsForSmtp;
            UseSmtpCredentialsRadioButton.IsChecked = !account.UseImapCredentialsForSmtp;

            App.Context.FlyOutClosing += OnFlyOutClosing;
            App.Context.FlyOutClosed += OnFlyOutClosed;
        }

        private void OnFlyOutClosing(object sender, EventArgs e) {
            SmtpPasswordBox.PasswordChanged -= OnSmtpPasswordChanged;
            ImapPasswordBox.PasswordChanged -= OnImapPasswordChanged;
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
    }
}