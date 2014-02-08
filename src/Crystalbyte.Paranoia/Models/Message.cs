using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Models {
    public class Message {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Mailbox")]
        public int MailboxId { get; set; }
        public virtual Mailbox Mailbox { get; set; }
        public string Subject { get; set; }
    }
}
