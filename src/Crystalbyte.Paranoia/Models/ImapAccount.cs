using Crystalbyte.Paranoia.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crystalbyte.Paranoia.Models {
    public class ImapAccount {
        public int Id { get; set; }
        public int IdentityId { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public short Port { get; set; }
        public SecurityPolicy Security { get; set; }
        public virtual List<Mailbox> Mailboxes { get; set; }
    }
}

