#region Using directives

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace Crystalbyte.Paranoia.Data {
    [Table("public_key")]
    public class PublicKeyModel {
        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        [Column("contact_id")]
        [ForeignKey("Contact")]
        public Int64 ContactId { get; set; }

        [Column("data")]
        public string Data { get; set; }

        public MailContactModel Contact { get; set; }
    }
}