using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crystalbyte.Paranoia.Data.SQLite;

namespace Crystalbyte.Paranoia.Data {
    [Table("mail_content")]
    [Virtual(ModuleType.Fts3)]
    internal class MailContent {

        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        [Column("subject")]
        public string Subject { get; set; }

        [Column("text")]
        public string Text { get; set; }

        [Column("mime")]
        public byte[] Mime { get; set; }

        [Column("message_id")]
        public Int64 MessageId { get; set; }
    }
}
