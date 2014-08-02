﻿#region Using directives

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace Crystalbyte.Paranoia.Data {
    [Table("smtp_request")]
    public class SmtpRequestModel {

        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        [Column("account_id")]
        [ForeignKey("Account")]
        public Int64 AccountId { get; set; }

        [Column("mime")]
        public string Mime { get; set; }

        [Column("to_address")]
        public string ToAddress { get; set; }

        public MailAccountModel Account { get; set; }
    }
}