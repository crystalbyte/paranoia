using System;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class DropMailboxCommand : ICommand {
        private readonly MailboxContext _mailbox;

        public DropMailboxCommand(MailboxContext mailbox) {
            _mailbox = mailbox;
            _mailbox.AssignmentChanged += (sender, e) => OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter) {
            return _mailbox.IsAssigned;
        }

        public async void Execute(object parameter) {
            await _mailbox.DropAsync();
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null) 
                handler(this, EventArgs.Empty);
        }
    }
}
