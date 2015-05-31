using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crystalbyte.Paranoia.Data.SQLite;

namespace Crystalbyte.Paranoia.Data {
    
    [Table("mail_address")]
    internal class MailAddress {

        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        [Index]
        [Column("name")]
        [Collate(CollatingSequence.NoCase)]
        public string Name { get; set; }

        [Index]
        [Column("mail_address")]
        [Collate(CollatingSequence.NoCase)]
        public string Address { get; set; }

        [Index]
        [Column("message_id")]
        public Int64 MessageId { get; set; }

        [Index]
        [Column("role")]
        public AddressRole Role { get; set; }

        [ForeignKey("MessageId")]
        public MailMessage Message { get; set; }
    }
}
