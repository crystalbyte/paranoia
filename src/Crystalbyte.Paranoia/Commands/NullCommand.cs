using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.Commands {
    public sealed class NullCommand : ICommand {
        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return false;
        }

        public void Execute(object parameter) {
            // Nada
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion
    }
}
