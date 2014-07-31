using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class MarkAsSeenCommand : ICommand {
        private readonly AppContext _context;

        public MarkAsSeenCommand(AppContext context) {
            _context = context;
            _context.MessageSelectionChanged += (sender, e) => OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter) {
            return _context.SelectedMessages != null
                   && _context.SelectedMessages.Any();
        }

        public void Execute(object parameter) {
            _context.MarkMessagesAsSeenAsync();
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null) 
                handler(this, EventArgs.Empty);
        }
    }
}
