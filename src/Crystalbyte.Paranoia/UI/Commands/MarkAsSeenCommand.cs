#region Using directives

using System;
using System.Linq;
using System.Windows.Input;

#endregion

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class MarkAsSeenCommand : ICommand {
        private readonly AppContext _context;

        public MarkAsSeenCommand(AppContext context) {
            _context = context;
            _context.MessageSelectionChanged += (sender, e) => OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter) {
            return _context.SelectedMessages != null
                   && _context.SelectedMessages.Any(x => x.IsNotSeen);
        }

        public async void Execute(object parameter) {
            await _context.MarkSelectionAsSeenAsync();
            OnCanExecuteChanged();
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}