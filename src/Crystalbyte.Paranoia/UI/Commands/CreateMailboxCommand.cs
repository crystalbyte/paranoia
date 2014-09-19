using System;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class CreateMailboxCommand : ICommand {
        private readonly MailboxContext _mailbox;

        public CreateMailboxCommand(MailboxContext mailbox) {
            _mailbox = mailbox;
        }

        public bool CanExecute(object parameter) {
            return _mailbox.CanHaveChildren;
        }

        public void Execute(object parameter) {
            throw new NotImplementedException();
        }

        public event EventHandler CanExecuteChanged;
    }
}
