#region Using directives

using System.Collections.Generic;
using System.Linq;
using Crystalbyte.Paranoia.Data;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class NotificationWindowContext : NotificationObject {
        private readonly MailMessageContext _message;

        public NotificationWindowContext(ICollection<MailMessageContext> messages) {
            if (messages.Count > 0) {
                _message = messages.First();
            }
        }

        public string FromAddress {
            get { return _message.FromAddress; }
        }

        public string FromName {
            get { return _message.FromName; }
        }

        public string Subject {
            get { return _message.Subject; }
        }
    }
}