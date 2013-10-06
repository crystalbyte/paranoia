using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.Commands {
    [Export, Shared]
    public sealed class ComposeMessageCommand : ICommand {

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            MessageBox.Show("Not implemented yet.");
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
