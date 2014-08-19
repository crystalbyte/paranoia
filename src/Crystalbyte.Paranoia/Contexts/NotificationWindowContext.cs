#region Using directives

using System.Collections.Generic;
using System.Linq;
using Crystalbyte.Paranoia.Data;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class NotificationWindowContext : NotificationObject {
        private IList<MailMessageModel> _messages;
        private readonly MailMessageModel _message;

        public NotificationWindowContext(IList<MailMessageModel> messages) {
            _messages = messages;
            if (messages.Count > 0) {
                _message = messages.First();
            }
        }


        public MimeMessageModel Message { get; set; }

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