using System;
using System.Windows;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class PrintCommand : ICommand {
        private readonly AppContext _appContext;

        public PrintCommand(AppContext appContext) {
            _appContext = appContext;
            _appContext.MessageSelectionChanged += OnMessageSelectionChanged;
        }

        private void OnMessageSelectionChanged(object sender, EventArgs e) {
            OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter) {
            return _appContext.SelectedMessage != null;
        }

        public void Execute(object parameter) {
            MessageBox.Show("Not yet implemented.");
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null) 
                handler(this, EventArgs.Empty);
        }
    }
}
