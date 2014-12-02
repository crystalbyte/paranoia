using System;
using System.Linq;
using System.Windows.Input;
using NLog;

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class RestoreMessagesCommand : ICommand {

        #region Private Fields

        private readonly AppContext _app;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public RestoreMessagesCommand(AppContext app) {
            _app = app;
            _app.MessageSelectionChanged += (sender, e) => OnCanExecuteChanged();
        }

        #endregion

        public bool CanExecute(object parameter) {
            // Group by accounts, since not all messages must necessarily be from the same account.
            var mailboxes = _app.SelectedMessages.GroupBy(x => x.Mailbox).ToArray();

            // We have found a trashbin for all accounts the messages belong too.
            return mailboxes.All(x => x.Key.IsTrash);
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null) 
                handler(this, EventArgs.Empty);
        }

        public async void Execute(object parameter) {
            try {
                await _app.RestoreSelectedMessagesAsync();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }
    }
}
