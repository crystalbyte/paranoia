using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crystalbyte.Paranoia.Data {
    [Table("mailbox_flag")]
    internal class MailboxFlag {

        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        [Index]
        [Column("mailbox_id")]
        public Int64 MailboxId { get; set; }

        [Index]
        [Column("value")]
        public string Value { get; set; }

        [ForeignKey("MailboxId")]
        public Mailbox Mailbox { get; set; }
    }
}
