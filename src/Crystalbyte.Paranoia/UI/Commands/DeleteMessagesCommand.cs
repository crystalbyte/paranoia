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

#endregion

namespace Crystalbyte.Paranoia.UI.Commands {
    public sealed class DeleteMessagesCommand : ICommand {

        #region Private Fields

        private readonly MailModule _module;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public DeleteMessagesCommand(MailModule module) {
            _module = module;
            _module.MessageSelectionChanged += OnMessageSelectionChanged;
        }

        #endregion

        private void OnMessageSelectionChanged(object sender, EventArgs e) {
            OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter) {
            // Group by accounts, since not all messages must necessarily be from the same account.
            var accounts = _module.SelectedMessages.GroupBy(x => x.Mailbox.Account).ToArray();
            var trashbins = accounts.Select(x => x.Key.Mailboxes.FirstOrDefault(y => y.IsTrash)).ToArray();

            // We have found a trashbin for all accounts the messages belong too.
            return trashbins.All(x => x != null);
        }

        public async void Execute(object parameter) {
            try {
                var selection = _module.SelectedMessages.ToArray();
                await _module.DeleteMessagesAsync(selection);
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
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