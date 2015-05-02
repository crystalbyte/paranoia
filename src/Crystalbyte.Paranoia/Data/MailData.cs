using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crystalbyte.Paranoia.Data {
    [Table("mail_data")]
    internal class MailData {

        [Key]
        [Index]
        [Column("id")]
        public Int64 Id { get; set; }

        [Column("mime")]
        public byte[] Mime { get; set; }

        [Column("message_id")]
        [ForeignKey("Message")]
        public Int64 MessageId { get; set; }

        public MailMessage Message { get; set; }
    }
}
