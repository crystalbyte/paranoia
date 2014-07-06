using System;
using Crystalbyte.Paranoia.Data;

namespace Crystalbyte.Paranoia {
    public class MailMessageContext : SelectionObject {
        private readonly MailMessageModel _message;

        public MailMessageContext(MailMessageModel message) {
            _message = message;
        }

        public string Subject {
            get { return _message.Subject; }
        }

        public DateTime EntryDate {
            get { return _message.EntryDate; }
        }

        public string FromName {
            get { return _message.FromName; }
        }

        public string FromAddress {
            get { return _message.FromAddress; }
        }
    }
}
