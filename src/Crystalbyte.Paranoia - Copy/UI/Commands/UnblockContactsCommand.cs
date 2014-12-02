using System;
using System.Linq;
using System.Windows.Input;
using NLog;

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class UnblockContactsCommand : ICommand {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly AppContext _app;

        #endregion

        public UnblockContactsCommand(AppContext app) {
            _app = app;
            _app.ContactSelectionChanged += (sender, e) => OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter) {
            return _app.SelectedContact != null && _app.SelectedContacts.Any(x => x.IsBlocked);
        }

        public async void Execute(object parameter) {
            try {
                await _app.UnblockSelectedUsersAsync();
            }
            catch (Exception ex) {
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