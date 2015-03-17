#region Copyright Notice & Copying Permission

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

using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Crystalbyte.Paranoia.Data;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class QueryContext : NotificationObject, IMessageSource {
        #region Private Fields

        private readonly string _query;
        private bool _isLoadingMessages;

        #endregion

        #region Construction

        public QueryContext(string query) {
            _query = query;
        }

        #endregion

        #region Properties

        public bool IsLoadingMessages {
            get { return _isLoadingMessages; }
            set {
                if (_isLoadingMessages == value) {
                    return;
                }
                _isLoadingMessages = value;
                RaisePropertyChanged(() => IsLoadingMessages);
            }
        }

        #endregion

        #region Implementation of IMessageSource

        public async Task<IEnumerable<MailMessageContext>> GetMessagesAsync() {
            Application.Current.AssertBackgroundThread();

            var mailbox = App.Context.SelectedMailbox;
            using (var database = new DatabaseContext()) {
                var messages = await database.MailMessages
                    .Where(x => x.MailboxId == mailbox.Id)
                    .Where(x => x.Subject.Contains(_query)
                                || x.FromAddress.Contains(_query)
                                || x.FromName.Contains(_query))
                    .ToArrayAsync();

                return messages.Select(x => new MailMessageContext(mailbox, x));
            }
        }

        #endregion
    }
}