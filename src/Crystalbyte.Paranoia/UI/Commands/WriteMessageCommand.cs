using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class WriteMessageCommand : ICommand {

        #region Private Fields

        private readonly AppContext _app;

        #endregion

        #region Construction

        public WriteMessageCommand(AppContext app) {
            _app = app;
            _app.OverlayChanged += (sender, e) => OnCanExecuteChanged();
            _app.MessageSelectionChanged += (sender, e) => OnCanExecuteChanged();
        }

        #endregion

        public bool CanExecute(object parameter) {
            return !_app.IsOverlayed && _app.SelectedMessage != null;
        }

        public void Execute(object parameter) {
            _app.IsOverlayed = true;
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null) 
                handler(this, EventArgs.Empty);
        }
    }
}
