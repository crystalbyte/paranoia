using System;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class RelayCommand : ICommand {

        #region Private Fields

        private readonly Action<object> _onExecute;
        private readonly Predicate<object> _onCanExecute;

        #endregion

        #region Construction

        public RelayCommand(Action<object> onExecute) {
            _onExecute = onExecute;
        }

        public RelayCommand(Predicate<object> onCanExecute, Action<object> onExecute)
            : this(onExecute) {
            _onCanExecute = onCanExecute;
        }

        #endregion

        #region Event Declarations

        public event EventHandler CanExecuteChanged;

        internal void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion

        public Predicate<object> OnCanExecute {
            get { return _onCanExecute; }
        }

        public Action<object> OnExecute {
            get { return _onExecute; }
        }

        public bool CanExecute(object parameter) {
            return _onCanExecute == null || _onCanExecute(parameter);
        }

        public void Execute(object parameter) {
            if (_onExecute == null) {
                return;
            }
            _onExecute(parameter);
        }
    }
}
