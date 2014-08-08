using System;
using System.Collections.Generic;
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
using Crystalbyte.Paranoia.Mail;

namespace Crystalbyte.Paranoia.UI.Pages {
    /// <summary>
    /// Interaction logic for AccountDetailsPage.xaml
    /// </summary>
    public partial class AccountDetailsPage {
        public AccountDetailsPage() {
            DataContext = App.Context.SelectedAccount;
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(PageCommands.Commit, OnPageCommit));
            CommandBindings.Add(new CommandBinding(PageCommands.Cancel, OnPageCancel));
        }

        private void OnPageCancel(object sender, ExecutedRoutedEventArgs e) {
            App.Context.CloseOverlay();
        }

        private void OnPageCommit(object sender, ExecutedRoutedEventArgs e) {
            
        }

        private void OnImapSecurityProtocolSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var account = (MailAccountContext) DataContext;
            account.ImapPort = (short) (account.ImapSecurity == SecurityProtocol.Implicit ? 993 : 143);
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
    }
}
