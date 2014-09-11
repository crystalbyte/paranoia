#region Using directives

using System;
using System.Linq;
using System.Windows.Input;
using NLog;

#endregion

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class DeleteMessagesCommand : ICommand {

        #region Private Fields

        private readonly AppContext _app;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public DeleteMessagesCommand(AppContext app) {
            _app = app;
            _app.MessageSelectionChanged += OnMessageSelectionChanged;
        }

        #endregion

        private void OnMessageSelectionChanged(object sender, EventArgs e) {
            OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter) {

            // Group by accounts, since not all messages must necessarily be from the same account.
            var accounts = _app.SelectedMessages.GroupBy(x => x.Mailbox.Account).ToArray();
            var trashbins = accounts.Select(x => x.Key.Mailboxes.FirstOrDefault(y => y.IsTrash)).ToArray();

            // We have found a trashbin for all accounts the messages belong too.
            return trashbins.All(x => x != null);
        }

        public async void Execute(object parameter) {
            try {
                await _app.DeleteSelectedMessagesAsync();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}