using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crystalbyte.Paranoia.Data.SQLite;

namespace Crystalbyte.Paranoia.Data {
    [Table("mail_attachment")]
    internal sealed class MailAttachment {

        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        [Column("filename")]
        [Collate(CollatingSequence.NoCase)]
        public string Filename { get; set; }

        [Column("size")]
        public Int64 Size { get; set; }

        [Index]
        [Column("content_id")]
        public string ContentId { get; set; }

        [Index]
        [Column("content_type")]
        public string ContentType { get; set; }

        [Column("content_disposition")]
        public string ContentDisposition { get; set; }

        [Index]
        [Column("message_id")]
        public Int64 MessageId { get; set; }

        [ForeignKey("MessageId")]
        public MailMessage Message { get; set; }
    }
}
