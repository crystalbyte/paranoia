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
using System.Data;
using System.Data.SQLite;
using System.Linq;
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
            foreach (var mailbox in App.Context.Accounts.SelectMany(x => x.Mailboxes)) {
                _mailboxes.Add(mailbox.Id, mailbox);
            }
        }
        
        public async Task<IEnumerable<MailMessageContext>> GetMessagesAsync() {
            Application.Current.AssertBackgroundThread();

            using (var context = new DatabaseContext()) {
                await context.OpenAsync();

                const string match = "SELECT * FROM mail_content WHERE mail_content MATCH @query;";
                var query = new SQLiteParameter("@query", _query);
                var result = await context.Database.SqlQuery<QueryResult>(match, query).ToArrayAsync();

                if (result.Length == 0) {
                    return new MailMessageContext[0];
                }

                // Strip out duplicates.
                var ids = result.Select(x => x.message_id).Distinct().ToArray();

                // We need to read all messages found by the full text search.
                // EF should not be used, the fastest way seems to be to query the database manually using ADO.
                // http://stackoverflow.com/questions/8107439/entity-framework-4-1-most-efficient-way-to-get-multiple-entities-by-primary-key
                var parameters = ids.Select(x => string.Format("@p{0}", x));
                using (var command = context.Database.Connection.CreateCommand()) {
                    command.CommandText = string.Format("SELECT * FROM mail_message WHERE id IN ({0});", string.Join(", ", parameters));
                    command.Parameters.AddRange(ids.Select(x => new SQLiteParameter(string.Format("@p{0}", x), x)).ToArray());
                    command.CommandType = CommandType.Text;

                    var reader = command.ExecuteReader(CommandBehavior.Default);
                    var messages = reader.Read<MailMessage>();
                    return messages.Select(x => new MailMessageContext(_mailboxes[x.MailboxId], x)).ToArray();
                }
            }
        }

        public void FinishQuery() {
            Application.Current.AssertUIThread();
            _mailboxes.Clear();
        }

        #endregion

        // The class is instantiated by the entity provider and requires 
        // unconventional property names for the internal mapper to make correct assignments.

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
