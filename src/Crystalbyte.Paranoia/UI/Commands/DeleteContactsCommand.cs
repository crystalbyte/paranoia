using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using NLog;

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class DeleteContactsCommand : ICommand {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly AppContext _app;

        #endregion

        #region Construction

        public DeleteContactsCommand(AppContext app) {
            _app = app;
            _app.ContactSelectionChanged += (sender, e) => OnCanExecuteChanged();
        }

        #endregion

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return _app.SelectedContact != null;
        }

        public async void Execute(object parameter) {
            try {
                await _app.DeleteSelectedContactsAsync();
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
