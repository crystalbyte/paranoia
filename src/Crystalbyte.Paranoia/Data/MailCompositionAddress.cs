using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crystalbyte.Paranoia.Data {
    [Table("mail_composition_address")]
    internal class MailCompositionAddress {

        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("role")]
        public AddressRole Role { get; set; }

        [Index]
        [Column("composition_id")]
        public long CompositionId { get; set; }

        [ForeignKey("CompositionId")]
        public MailComposition MailComposition { get; set; }
    }
}
