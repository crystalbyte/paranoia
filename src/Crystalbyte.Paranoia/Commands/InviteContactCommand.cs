using Crystalbyte.Paranoia.Messaging;
using System;
using System.Composition;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.Commands {
    [Export, Shared]
    public sealed class InviteContactCommand : ICommand {

        #region Import Declarations

        [Import]
        public IdentitySelectionSource IdentitySelectionSource { get; set; }

        [Import]
        public ContactSelectionSource ContactSelectionSource { get; set; }

        [Import]
        public ImapAccountSelectionSource ImapAccountSelectionSource { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            IdentitySelectionSource.SelectionChanged += (sender, e) => OnCanExecuteChanged(EventArgs.Empty);
            ContactSelectionSource.SelectionChanged += (sender, e) => OnCanExecuteChanged(EventArgs.Empty);
        }

        #endregion

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return IdentitySelectionSource.Current != null
                && ContactSelectionSource.Current != null
                && ContactSelectionSource.Current.RequestStatus != ContactRequestStatus.Accepted;
        }

        public void Execute(object parameter) {
            SendInviteAsync();
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        private async Task SendInviteAsync() {
            var account = ImapAccountSelectionSource.Current;
            var host = account.SmtpHost;
            var port = account.SmtpPort;

            using (var connection = new SmtpConnection()) {
                using (var authenticator = await connection.ConnectAsync(host, port)) {
                    
                }
            }
        }
    }
}
