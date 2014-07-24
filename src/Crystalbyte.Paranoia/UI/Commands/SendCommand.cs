using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class SendCommand  : ICommand{
        private readonly MailCompositionContext _composition;

        public SendCommand(MailCompositionContext composition) {
            _composition = composition;
        }

        public bool CanExecute(object parameter) {
            return !string.IsNullOrEmpty(_composition.Subject)
                   || !_composition.Recipients.Any();
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null) 
                handler(this, EventArgs.Empty);
        }

        public void Execute(object parameter) {
            MessageBox.Show("Not yet implemented.");
        }
    }
}
