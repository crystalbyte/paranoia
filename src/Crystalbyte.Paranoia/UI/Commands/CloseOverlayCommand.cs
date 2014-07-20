using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class CloseOverlayCommand : ICommand
    {
        private AppContext _app;

        public CloseOverlayCommand(AppContext app) {
            _app = app;
            _app.OverlayChanged += (sender, e) => OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter) {
            return _app.IsOverlayed;
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
        }


        public void Execute(object parameter) {
            _app.IsOverlayed = false;
        }
    }
}
