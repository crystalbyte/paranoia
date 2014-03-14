using System;
using System.Composition;
using System.Windows.Media;
using Crystalbyte.Paranoia.Contexts;
using Crystalbyte.Paranoia.Properties;

namespace Crystalbyte.Paranoia.Commands {
    [Export, Shared]
    public sealed class CreateIdentityCommand  {

        #region Import Declarations

        [Import]
        public AppContext AppContext { get; set; }

        #endregion

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter) {
            AppContext.ShowIdentityCreationScreen();
        }

        #endregion

        public ImageSource SmallImageSource {
            get { return null; }
        }

        public string Text {
            get { return Resources.CreateIdentityCommandText; }
        }

        public string RibbonPath {
            get { return "Identity/Action"; }
        }

        public ImageSource LargeImageSource {
            get { return null; }
        }
    }
}
