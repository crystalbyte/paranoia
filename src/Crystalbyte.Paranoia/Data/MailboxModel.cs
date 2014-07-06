using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crystalbyte.Paranoia.Data {

    [Table("mailbox")]
    public class MailboxModel {
        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("delimiter")]
        public char Delimiter { get; set; }

        [Column("flags")]
        public string Flags { get; set; }

        [Column("type")]
        public MailboxType Type { get; set; }

        [Column("account_id")]
        [ForeignKey("Account")]
        public Int64 AccountId { get; set; }

        public virtual MailAccountModel Account { get; set; }
    }
}
