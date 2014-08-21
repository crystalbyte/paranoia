using System;
using System.Linq;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class RestoreMessageCommand : ICommand {
        private readonly MailAccountContext _account;

        public RestoreMessageCommand(MailAccountContext account) {
            _account = account;
            _account.MailboxSelectionChanged += (sender, e) => OnCanExecuteChanged();
            App.Context.MessageSelectionChanged += (sender, e) => OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter) {
            return _account.SelectedMailbox.IsTrash
                   && App.Context.SelectedMessages.Any();
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public async void Execute(object parameter) {
            try {
                var messages = App.Context.SelectedMessages.ToArray();
                await _account.RestoreMessagesAsync(messages);        
            }
            catch (Exception) {
                throw;
            }
        }
    }
}
