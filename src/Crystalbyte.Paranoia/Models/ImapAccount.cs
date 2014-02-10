using Crystalbyte.Paranoia.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Crystalbyte.Paranoia.Models {
    public class ImapAccount {
        [Key, ForeignKey("Identity")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdentityId { get; set; }
        [StringLength(256)]
        public string Username { get; set; }
        [StringLength(256)]
        public string Password { get; set; }
        [StringLength(256)]
        public string Host { get; set; }
        public short Port { get; set; }
        public SecurityPolicy Security { get; set; }
        public virtual List<Mailbox> Mailboxes { get; set; }
        public virtual Identity Identity { get; set; }
    }
}

