using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crystalbyte.Paranoia.Data.SQLite;

namespace Crystalbyte.Paranoia.Data {
    [Table("mail_message_flag")]
    internal class MailMessageFlag {

        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        [Index]
        [Column("message_id")]
        public Int64 MessageId { get; set; }

        [Index]
        [Column("value")]
        [Collate(CollatingSequence.NoCase)]
        public string Value { get; set; }

        [ForeignKey("MessageId")]
        public MailMessage Message { get; set; }
    }
}
