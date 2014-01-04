﻿#region Using directives

using System;
using System.Composition;
using System.Windows.Input;
using Crystalbyte.Paranoia.Contexts;

#endregion

namespace Crystalbyte.Paranoia.Commands {
    [Export, Shared]
    public sealed class SyncCommand : ICommand {
        [Import]
        public AppContext AppContext { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            AppContext.SyncStatusChanged += (sender, e) => OnCanExecuteChanged(EventArgs.Empty);
        }

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return !AppContext.IsSyncing;
        }

        public void Execute(object parameter) {
            AppContext.SyncAsync();
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