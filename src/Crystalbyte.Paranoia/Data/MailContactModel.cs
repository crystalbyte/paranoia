#region Using directives

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace Crystalbyte.Paranoia.Data {
    [Table("mail_contact")]
    public class MailContactModel {
        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        [Column("name")]
        [Collate(CollatingSequence.NoCase)]
        public string Name { get; set; }

        [Column("address")]
        [Collate(CollatingSequence.NoCase)]
        public string Address { get; set; }

        [Column("public_key")]
        public string PublicKey { get; set; }

        [Column("account_id")]
        [ForeignKey("Account")]
        public Int64 AccountId { get; set; }

        public virtual MailAccountModel Account { get; set; }
    }
}