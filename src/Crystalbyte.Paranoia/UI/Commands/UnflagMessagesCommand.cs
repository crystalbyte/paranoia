﻿#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using Directives

using System;
using System.Linq;
using System.Windows.Input;

#endregion

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class UnflagMessagesCommand : ICommand {
        #region Private Fields

        private readonly AppContext _context;

        #endregion

        #region Construction

        public UnflagMessagesCommand(AppContext context) {
            _context = context;
            _context.MessageSelectionChanged += (sender, e) => OnCanExecuteChanged();
        }

        #endregion

        public bool CanExecute(object parameter) {
            return _context.SelectedMessage != null
                   && _context.SelectedMessages.Any(x => x.IsFlagged);
        }

        public async void Execute(object parameter) {
            var messages = _context.SelectedMessages.ToArray();
            await _context.UnflagMessagesAsync(messages);
            OnCanExecuteChanged();
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}