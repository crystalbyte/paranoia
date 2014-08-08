using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Crystalbyte.Paranoia.Contexts;
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
            App.Context.OverlayClosed -= OnOverlayClosed;
            SmtpPasswordBox.PasswordChanged -= OnSmtpPasswordChanged;
            ImapPasswordBox.PasswordChanged -= OnImapPasswordChanged;

            if (_discardOnClose) {
                DiscardChanged();
            }
        }

        private static void OnPageCancel(object sender, ExecutedRoutedEventArgs e) {
            App.Context.CloseOverlay();
        }

        private void DiscardChanged() {
            _tracker.Stop();
            _tracker.Revert();
        }

        private async void OnPageCommit(object sender, ExecutedRoutedEventArgs e) {
            await SaveChanges();
            App.Context.CloseOverlay();
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
            App.Context.OverlayClosed += OnOverlayClosed;
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
