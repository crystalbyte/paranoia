#region Using directives

using System;
using System.Windows.Input;

#endregion

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class ReplyCommand : ICommand {
        private readonly AppContext _app;

        public ReplyCommand(AppContext app) {
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
            _app.OnReplyToMessage();
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}