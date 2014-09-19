using System;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class DeleteMailboxCommand : ICommand {
        private readonly MailboxContext _mailbox;

        public DeleteMailboxCommand(MailboxContext mailbox) {
            _mailbox = mailbox;
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public async void Execute(object parameter) {
            await _mailbox.DeleteAsync();
        }

        public event EventHandler CanExecuteChanged;
    }
}
