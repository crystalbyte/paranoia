#region Using directives

using System;
using System.Linq;
using System.Windows.Input;
using NLog;

#endregion

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class DeleteMessageCommand : ICommand {

        #region Private Fields

        private readonly AppContext _app;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public DeleteMessageCommand(AppContext app) {
            _app = app;
            _app.MessageSelectionChanged += OnMessageSelectionChanged;
        }

        #endregion

        private void OnMessageSelectionChanged(object sender, EventArgs e) {
            OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter) {
            return false;
            //return _app.SelectedAccount != null
            //       && _app.SelectedMessage != null
            //       && _app.SelectedAccount.DockedMailboxes.FirstOrDefault(x => x.Type == MailboxType.Trash) != null;
        }

        public async void Execute(object parameter) {
            //try {
            //    var trash = _app.SelectedAccount.DockedMailboxes.FirstOrDefault(x => x.Type == MailboxType.Trash);
            //    if (trash == null) {
            //        return;
            //    }

            //    var messages = _app.SelectedMessages.ToArray();
            //    var mailbox = _app.SelectedAccount.SelectedMailbox;
            //    await mailbox.DeleteMessagesAsync(messages, trash.Name);
            //}
            //catch (Exception ex) {
            //    Logger.Error(ex);
            //}
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}