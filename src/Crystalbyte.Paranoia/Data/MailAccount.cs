using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crystalbyte.Paranoia.Data {
    [Table("mail_account")]
    public class MailAccount {
        private ICollection<MailContact> _contacts;

        public MailAccount() {
            _contacts = new Collection<MailContact>();
        }

        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("imap_host")]
        public string ImapHost { get; set; }

        [Column("imap_port")]
        public short ImapPort { get; set; }

        [Column("imap_username")]
        public string ImapUsername { get; set; }

        [Column("imap_password")]
        public string ImapPassword { get; set; }

        [Column("imap_security")]
        public byte ImapSecurity { get; set; }

        [Column("smtp_host")]
        public string SmtpHost { get; set; }

        [Column("smtp_port")]
        public Int16 SmtpPort { get; set; }

        [Column("smtp_username")]
        public string SmtpUsername { get; set; }

        [Column("smtp_password")]
        public string SmtpPassword { get; set; }

        [Column("smtp_security")]
        public byte SmtpSecurity { get; set; }

        [Column("smtp_require_auth")]
        public bool SmtpRequiresAuthentication { get; set; }

        [Column("use_imap_credentials")]
        public bool UseImapCredentialsForSmtp { get; set; }

        public virtual ICollection<MailContact> Contacts {
            get { return _contacts; }
            set { _contacts = value; }
        }
    }
}
