#region Using directives

using System;
using System.Windows.Input;

#endregion

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class DropAssignmentCommand : ICommand {
        private readonly MailAccountContext _account;

        public DropAssignmentCommand(MailAccountContext account) {
            _account = account;
            _account.MailboxSelectionChanged += (sender, e) => OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter) {
            var mailbox = _account.SelectedMailbox;
            return mailbox != null && mailbox.IsAssigned;
        }

        public async void Execute(object parameter) {
            var mailbox = _account.SelectedMailbox;
            await mailbox.DropAssignmentAsync();
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}