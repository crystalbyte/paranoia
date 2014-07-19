#region Using directives

using System;
using System.Windows;
using System.Windows.Input;

#endregion

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class ForwardCommand : ICommand {
        private readonly AppContext _app;

        public ForwardCommand(AppContext app) {
            _app = app;
            _app.MessageSelectionChanged += OnMessageSelectionChanged;
        }

        private void OnMessageSelectionChanged(object sender, EventArgs e) {
            OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter) {
            return _app.SelectedMessage != null;
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