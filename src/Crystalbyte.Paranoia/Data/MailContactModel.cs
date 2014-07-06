using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crystalbyte.Paranoia.Data {
    [Table("mail_contact")]
    public class MailContactModel {

        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("account_id")]
        [ForeignKey("Account")]
        public Int64 AccountId { get; set; }

        public virtual MailAccountModel Account { get; set; }
    }
}
