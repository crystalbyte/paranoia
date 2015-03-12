using System;
using System.Linq;
using System.Windows.Input;
using NLog;

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class BlockContactsCommand : ICommand {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly AppContext _app;

        #endregion

        #region Construction

        public BlockContactsCommand(AppContext app) {
            _app = app;
            _app.ContactSelectionChanged += (sender, e) => OnCanExecuteChanged();
        }

        #endregion

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return _app.SelectedContact != null && _app.SelectedContacts.Any(x => !x.IsIgnored);
        }

        public async void Execute(object parameter) {
            try {
                await _app.BlockSelectedUsersAsync();
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

        #endregion
    }
}