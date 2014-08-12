using Crystalbyte.Paranoia.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Contexts
{
    public sealed class NotificationWindowContext:NotificationObject
    {
        private IList<MailMessageModel> _messages;
        private MailMessageModel _message;

        public NotificationWindowContext(IList<MailMessageModel> messages)
        {
            _messages = messages;
            if (messages.Count == 1)
            {
                _message = messages.First();
            }

        }


        public MimeMessageModel Message { get; set; }
        public String FromAddress { get { return _message.FromAddress.ToString(); } set{} }
        public String FromName { get { return _message.FromName.ToString();} set{} }
        public String Subject { get { return _message.Subject.ToString(); } set {} }
    }
}
