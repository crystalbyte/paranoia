using Crystalbyte.Paranoia.Contexts;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Models;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.Commands {
    [Export, Shared]
    public sealed class DeleteIdentityCommand : ICommand {

        #region Import Declarations

        [Import]
        public IdentitySelectionSource IdentitySelectionSource { get; set; }

        [Import]
        public AppContext AppContext { get; set; }

        #endregion

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return parameter is IdentityContext
                || IdentitySelectionSource.Selection != null;
        }

        public event EventHandler CanExecuteChanged;
        public void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
        }

        public async void Execute(object parameter) {
            var id = parameter as IdentityContext;
            if (id == null) {
                id = IdentitySelectionSource.Selection;
            }

            await id.DeleteAsync();
            await AppContext.QueryIdentitiesAsync();
        }

        #endregion
    }
}
