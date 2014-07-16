#region Using directives

using System;
using System.Linq;
using System.Windows.Input;

#endregion

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class DeleteMessageCommand : ICommand {
        private readonly AppContext _app;

        public DeleteMessageCommand(AppContext app) {
            _app = app;
            _app.MessageSelectionChanged += OnMessageSelectionChanged;
        }

        private void OnMessageSelectionChanged(object sender, EventArgs e) {
            OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter) {
            return _app.SelectedAccount != null
                   && _app.SelectedMessage != null
                   && _app.SelectedAccount.Mailboxes.FirstOrDefault(x => x.Type == MailboxType.Trash) != null;
        }

        public async void Execute(object parameter) {
            var trash = _app.SelectedAccount.Mailboxes.FirstOrDefault(x => x.Type == MailboxType.Trash);
            if (trash == null) {
                return;
            }

            var messages = _app.SelectedMessages.ToArray();
            var mailbox = _app.SelectedAccount.SelectedMailbox;
            await mailbox.DeleteMessagesAsync(messages, trash.Name);
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}