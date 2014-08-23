#region Using directives

using System;
using System.Diagnostics;
using System.Windows.Input;
using NLog;

#endregion

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class ComposeMessageCommand : ICommand {

        #region Private Fields

        private readonly AppContext _app;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public ComposeMessageCommand(AppContext app) {
            _app = app;
            _app.MessageSelectionChanged += (sender, e) => OnCanExecuteChanged();
        }

        #endregion

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            try {
                _app.OnComposeMessage();
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}