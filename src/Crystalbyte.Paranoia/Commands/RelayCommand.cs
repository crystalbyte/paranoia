#region Using directives

using System;
using System.Windows.Input;

#endregion

namespace Crystalbyte.Paranoia.Commands {
    public sealed class RelayCommand : ICommand {
        public RelayCommand(Func<object, bool> canExecuteCallback,
                            Action<object> executeCallback) {
            ExecuteCallback = executeCallback;
            CanExecuteCallback = canExecuteCallback;
        }

        public RelayCommand(Action<object> executeCallback)
            : this(x => true, executeCallback) { }

        public Func<object, bool> CanExecuteCallback { get; private set; }
        public Action<object> ExecuteCallback { get; private set; }

        public bool CanExecute(object parameter) {
            return CanExecuteCallback == null || CanExecuteCallback(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        public void Execute(object parameter) {
            if (ExecuteCallback != null) {
                ExecuteCallback(parameter);
            }
        }
    }
}