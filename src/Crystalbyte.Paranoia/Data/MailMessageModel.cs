using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crystalbyte.Paranoia.Data {

    [Table("mail_message")]
    public sealed class MailMessageModel {

        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        [Column("uid")]
        public Int64 Uid { get; set; }

        [Column("subject")]
        public string Subject { get; set; }

        [Column("entry_date")]
        public DateTime EntryDate { get; set; }

        [Column("thread_id")]
        public string ThreadId { get; set; }

        [Column("message_id")]
        public string MessageId { get; set; }

        [Column("from_name")]
        public string FromName { get; set; }

        [Column("from_address")]
        public string FromAddress { get; set; }

        [Column("mailbox_id")]
        [ForeignKey("Mailbox")]
        public Int64 MailboxId { get; set; }

        public MailboxModel Mailbox { get; set; }
    }
}
