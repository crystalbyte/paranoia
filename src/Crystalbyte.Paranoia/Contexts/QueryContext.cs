using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;

namespace Crystalbyte.Paranoia {
    public sealed class QueryContext : IMessageSource {

        #region Private Fields

        private readonly string _query;

        #endregion

        #region Construction

        public QueryContext(string query) {
            _query = query;
        }

        #endregion

        #region Implementation of IMessageSource

        public async Task<IEnumerable<MailMessageContext>> GetMessagesAsync() {
            var mailbox = App.Context.SelectedMailbox;
            IEnumerable<MailMessageModel> messages;
            using (var database = new DatabaseContext()) {
                messages = await database.MailMessages
                    .Where(x => x.MailboxId == mailbox.Id)
                    .Where(x => x.Subject.Contains(_query) 
                        || x.FromAddress.Contains(_query) 
                        || x.FromName.Contains(_query))
                    .ToArrayAsync();
            }
            return messages.Select(x => new MailMessageContext(mailbox, x));
        }

        #endregion
    }
}
