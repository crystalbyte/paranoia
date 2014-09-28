﻿using System;
using System.Linq;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class MarkAsFlaggedCommand : ICommand {

        #region Private Fields

        private readonly AppContext _context;

        #endregion

        #region Construction

        public MarkAsFlaggedCommand(AppContext context) {
            _context = context;
            _context.MessageSelectionChanged += (sender, e) => OnCanExecuteChanged();
        }

        #endregion

        public bool CanExecute(object parameter) {
            return _context.SelectedMessage != null
             && _context.SelectedMessages.Any(x => !x.IsFlagged);
        }

        public async void Execute(object parameter) {
            await _context.MarkSelectionAsFlaggedAsync();
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
