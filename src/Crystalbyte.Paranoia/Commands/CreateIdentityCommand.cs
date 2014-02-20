﻿using Crystalbyte.Paranoia.Contexts;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.Commands {
    [Export, Shared]
    public sealed class CreateIdentityCommand : ICommand {

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
    }
}
