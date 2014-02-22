using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Models;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class MailContactContext : NotificationObject {

        private readonly MailContact _contact;
        private readonly MailContext _mail;
        public MailContactContext(MailContext mail, MailContact contact) {
            _mail = mail;
            _contact = contact;
        }

        public string Name {
            get { return _contact.Name; }
        }

        public string Address {
            get { return _contact.Address; }
        }

        public MailContactType Type {
            get { return _contact.Type; }
        }
    }
}
