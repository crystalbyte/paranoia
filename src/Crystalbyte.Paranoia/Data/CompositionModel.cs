#region Using directives

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace Crystalbyte.Paranoia.Data {
    [Table("composition")]
    public class CompositionModel {
        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        [Column("account_id")]
        [ForeignKey("Account")]
        public Int64 AccountId { get; set; }

        [Column("mime")]
        public byte[] Mime { get; set; }

        [Column("subject")]
        public string Subject { get; set; }

        [Column("to_name")]
        public string ToName { get; set; }

        [Column("to_address")]
        public string ToAddress { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        public MailAccountModel Account { get; set; }
    }
}