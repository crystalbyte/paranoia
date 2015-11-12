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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Data.SQLite;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class MessageQuery : IMessageSource {

        #region Private Fields

        private readonly string _query;
        private readonly IDictionary<Int64, MailboxContext> _mailboxes;

        #endregion

        #region Construction

        public MessageQuery(string query) {
            _query = query;
            _mailboxes = new Dictionary<Int64, MailboxContext>();
        }

        #endregion

        #region Implementation of IMessageSource
        public void BeginQuery() {
            Application.Current.AssertUIThread();

            // Compose a new list of all possible mailboxes available. 
            // We will need them to map messages back into their own mailbox later.
            var module = App.Context.GetModule<MailModule>();
            foreach (var mailbox in module.Accounts.SelectMany(x => x.Mailboxes)) {
                _mailboxes.Add(mailbox.Id, mailbox);
            }
        }

        public async Task<IEnumerable<MailMessageContext>> GetMessagesAsync() {
            Application.Current.AssertBackgroundThread();

            var contentTableAttribute = typeof(MailMessageContent).GetCustomAttribute<TableAttribute>();
            var contentTableName = contentTableAttribute == null ? typeof(MailMessageContent).Name : contentTableAttribute.Name;

            using (var context = new DatabaseContext()) {
                await context.OpenAsync();

                const string param = "@query";
                var match = string.Format("SELECT * FROM {0} WHERE {0} MATCH {1};", contentTableName, param);
                var query = new SQLiteParameter(param, _query);
                var result = await context.Database.SqlQuery<QueryResult>(match, query).ToArrayAsync();

                if (result.Length == 0) {
                    return new MailMessageContext[0];
                }

                // Strip out duplicates.
                var ids = result.Select(x => x.message_id).Distinct().ToArray();

                var messages = await context.MailMessages
                    .Include(x => x.Attachments)
                    .Include(x => x.Addresses)
                    .Include(x => x.Flags)
                    .Where(x => ids.Contains(x.Id))
                    .AsNoTracking()
                    .ToArrayAsync();

                return messages.Select(x => new MailMessageContext(_mailboxes[x.MailboxId], x) {
                    IsAccountVisible = true
                }).ToArray();
            }
        }

        public void FinishQuery() {
            Application.Current.AssertUIThread();
            _mailboxes.Clear();
        }

        #endregion
        // ReSharper disable once ClassNeverInstantiated.Local
        private class QueryResult {
            // ReSharper disable UnusedMember.Local
            // ReSharper disable InconsistentNaming
            public string subject { get; set; }
            public string text { get; set; }
            public Int64 message_id { get; set; }
            // ReSharper restore InconsistentNaming
            // ReSharper restore UnusedMember.Local
        }
    }
}
