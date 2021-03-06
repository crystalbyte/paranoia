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
using NLog;
using NLog.Fluent;

#endregion

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class FlagMessagesAsUnseenCommand : ICommand {

        #region Private Fields

        private readonly MailModule _module;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public FlagMessagesAsUnseenCommand(MailModule module) {
            _module = module;
            _module.MessageSelectionChanged += (sender, e) => OnCanExecuteChanged();
        }

        #endregion

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return _module.SelectedMessages != null
                   && _module.SelectedMessages.Any(x => x.IsSeen);
        }

        public async void Execute(object parameter) {
            try {
                var messages = _module.SelectedMessages.ToArray();
                await _module.MarkMessagesAsUnseenAsync(messages);
                OnCanExecuteChanged();
            }
            catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        public event EventHandler CanExecuteChanged;

        private void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion
    }
}