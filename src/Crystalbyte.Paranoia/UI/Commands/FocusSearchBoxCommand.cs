using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class FocusSearchBoxCommand : ICommand {
        private readonly AppContext _app;
        private readonly Control _control;

        public FocusSearchBoxCommand(AppContext app, Control control) {
            _app = app;
            _control = control;
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            var arg = parameter as string;
            if (arg == "!") {
                _app.QueryString = string.Empty;
                _control.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                return;
            }
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
