using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Crystalbyte.Paranoia.Mail;

namespace Crystalbyte.Paranoia.UI.Pages {
    /// <summary>
    /// Interaction logic for AccountDetailsPage.xaml
    /// </summary>
    public partial class AccountDetailsPage : INavigationAware {

        private bool _discardOnClose;
        private RevisionTracker<MailAccountContext> _tracker;

        public AccountDetailsPage() {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(PageCommands.Commit, OnPageCommit));
            CommandBindings.Add(new CommandBinding(PageCommands.Cancel, OnPageCancel));
        }

        private void OnOverlayClosed(object sender, EventArgs e) {
            var account = (MailAccountContext) DataContext;
            account.Testing = null;

            App.Context.FlyOutClosed -= OnOverlayClosed;
            if (_discardOnClose) {
                DiscardChanged();
            }
        }

        private static void OnPageCancel(object sender, ExecutedRoutedEventArgs e) {
            App.Context.CloseFlyOut();
        }

        private void DiscardChanged() {
            _tracker.Stop();
            _tracker.Revert();
        }

        private async void OnPageCommit(object sender, ExecutedRoutedEventArgs e) {
            await SaveChanges();
            App.Context.CloseFlyOut();
        }

        private async Task SaveChanges() {
            _discardOnClose = false;
            _tracker.Stop();

            var account = (MailAccountContext)DataContext;
            await account.SyncToDatabaseAsync();
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
            var account = App.Context.SelectedAccount;

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
                .WithProperty(x => x.UseImapCredentialsForSmtp)
                .Start();

            DataContext = account;

            SmtpPasswordBox.Password = account.SmtpPassword;
            SmtpPasswordBox.PasswordChanged += OnSmtpPasswordChanged;

            ImapPasswordBox.Password = account.ImapPassword;
            ImapPasswordBox.PasswordChanged += OnImapPasswordChanged;

            UseImapCredentialsRadioButton.IsChecked = account.UseImapCredentialsForSmtp;
            UseSmtpCredentialsRadioButton.IsChecked = !account.UseImapCredentialsForSmtp;

            App.Context.FlyOutClosing += OnOverlayClosing;
            App.Context.FlyOutClosed += OnOverlayClosed;
        }

        private void OnOverlayClosing(object sender, EventArgs e) {
            SmtpPasswordBox.PasswordChanged -= OnSmtpPasswordChanged;
            ImapPasswordBox.PasswordChanged -= OnImapPasswordChanged;
        }

        private void OnUseImapCredentialsChecked(object sender, RoutedEventArgs e) {
            var account = (MailAccountContext)DataContext;
            var button = ((RadioButton) sender);
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
