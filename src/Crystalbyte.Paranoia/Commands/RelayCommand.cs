using System;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.Commands {
    public sealed class RelayCommand : ICommand {

        public Func<object, bool> OnCanExecute { get; set; }
        public Action<object> OnExecute { get; set; }

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            if (OnCanExecute != null) {
                OnCanExecute(parameter);
            }
            return true;
        }

        public void Execute(object parameter) {
            if (OnExecute != null) {
                OnExecute(parameter);
            }
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
