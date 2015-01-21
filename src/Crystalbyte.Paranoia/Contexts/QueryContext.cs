using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Crystalbyte.Paranoia.Data;

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
