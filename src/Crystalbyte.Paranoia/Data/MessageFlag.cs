using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crystalbyte.Paranoia.Data.SQLite;

namespace Crystalbyte.Paranoia.Data {
    [Table("message_flag")]
    internal class MessageFlag {

        [Key]
        [Index]
        [Column("id")]
        public Int64 Id { get; set; }

        [Column("message_id")]
        [ForeignKey("Message")]
        public Int64 MessageId { get; set; }

        [Index]
        [Column("value")]
        [Collate(CollatingSequence.NoCase)]
        public string Value { get; set; }

        public MailMessage Message { get; set; }
    }
}
