using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crystalbyte.Paranoia.Models {
    public class Mail {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Mailbox")]
        public int MailboxId { get; set; }
        public string Subject { get; set; }
        public DateTime? InternalDate { get; set; }
        [Required]
        public long Size { get; set; }
        [Required]
        public long Uid { get; set; }
        [Required]
        public string MessageId { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }

        public virtual Mailbox Mailbox { get; set; }

        public virtual List<MailContact> MailContacts { get; set; }

        public virtual List<MailFlag> MailFlags { get; set; }
    }
}
