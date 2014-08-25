#region Using directives

using System;
using System.Windows.Input;

#endregion

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class DropMailboxBindingCommand : ICommand {
        private readonly MailboxContext _mailbox;

        public DropMailboxBindingCommand(MailboxContext mailbox) {
            _mailbox = mailbox;
        }

        public bool CanExecute(object parameter) {
            return _mailbox.IsBound;
        }

        public async void Execute(object parameter) {
            await _mailbox.DropAssignmentAsync();
        }

        public event EventHandler CanExecuteChanged;

        internal void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}