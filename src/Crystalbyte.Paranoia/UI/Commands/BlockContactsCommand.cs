using System;
using System.Windows.Input;
using NLog;

namespace Crystalbyte.Paranoia.UI.Commands {
    public class BlockContactsCommand : ICommand {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly AppContext _app;

        #endregion

        public BlockContactsCommand(AppContext app) {
            _app = app;
            _app.ContactSelectionChanged += (sender, e) => OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter) {
            return _app.SelectedContact != null;
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

        protected virtual void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null) 
                handler(this, EventArgs.Empty);
        }
    }
}