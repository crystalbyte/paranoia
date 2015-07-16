using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crystalbyte.Paranoia.Data.SQLite;

namespace Crystalbyte.Paranoia.Data {

    [Table("mail_composition_attachment")]
    internal class MailCompositionAttachment {

        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        [Column("name")]
        [Collate(CollatingSequence.NoCase)]
        public string Name { get; set; }

        [Column("bytes")]
        public byte[] Bytes { get; set; }

        [Index]
        [Column("composition_id")]
        public Int64 CompositionId { get; set; }

        [ForeignKey("CompositionId")]
        public MailComposition MailComposition { get; set; }
    }
}
