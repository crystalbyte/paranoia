using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class FocusSearchBoxCommand : ICommand {
        private readonly Control _control;

        public FocusSearchBoxCommand(Control control) {
            _control = control;
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            _control.Focus();
        }

        public event EventHandler CanExecuteChanged;

        internal void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null) 
                handler(this, EventArgs.Empty);
        }
    }
}
