#region Using directives

using System;
using System.Windows.Input;
using Crystalbyte.Paranoia.Mail;

#endregion

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class BindMailboxCommand : ICommand {
        private readonly MailboxContext _mailbox;

        public BindMailboxCommand(MailboxContext mailbox) {
            _mailbox = mailbox;
        }

        public bool CanExecute(object parameter) {
            if (parameter == null) {
                return false;
            }
            var info = (ImapMailboxInfo) parameter;
            return !_mailbox.IsAssigned && info.IsSelectable;
        }

        public async void Execute(object parameter) {
            var info = (ImapMailboxInfo) parameter;
            await _mailbox.BindMailboxAsync(info);
        }

        public event EventHandler CanExecuteChanged;

        internal void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}