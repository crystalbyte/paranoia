using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using NLog;

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class RestoreCommand : ICommand {

        #region Private Fields

        private readonly AppContext _app;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public RestoreCommand(AppContext app) {
            _app = app;
            _app.MessageSelectionChanged += (sender, e) => OnCanExecuteChanged();
        }

        #endregion

        public bool CanExecute(object parameter) {
            return _app.SelectedMailbox != null
                && _app.SelectedMailbox.IsTrash
                && _app.SelectedMessages.Any();
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void Execute(object parameter) {
            try {
                var messages = _app.SelectedMessages.ToArray();
                var mailboxes = messages.GroupBy(x => x.Mailbox);
                Task.Run(() => mailboxes.ForEach(x => x.Key.RestoreMessagesAsync(x.ToList())));
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }
    }
}
