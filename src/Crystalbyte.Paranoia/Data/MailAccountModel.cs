#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crystalbyte.Paranoia.Mail;

#endregion

namespace Crystalbyte.Paranoia.Data {
    [Table("mail_account")]
    public class MailAccountModel {
        private ICollection<MailContactModel> _contacts;
        private ICollection<MailboxModel> _mailboxes;

        public MailAccountModel() {
            _contacts = new Collection<MailContactModel>();
            _mailboxes = new Collection<MailboxModel>();
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
        public SecurityPolicy ImapSecurity { get; set; }

        [Column("smtp_host")]
        public string SmtpHost { get; set; }

        [Column("smtp_port")]
        public Int16 SmtpPort { get; set; }

        [Column("smtp_username")]
        public string SmtpUsername { get; set; }

        [Column("smtp_password")]
        public string SmtpPassword { get; set; }

        [Column("smtp_security")]
        public SecurityPolicy SmtpSecurity { get; set; }

        [Column("smtp_require_auth")]
        public bool SmtpRequiresAuthentication { get; set; }

        [Column("use_imap_credentials")]
        public bool UseImapCredentialsForSmtp { get; set; }

        public virtual ICollection<MailContactModel> Contacts {
            get { return _contacts; }
            set { _contacts = value; }
        }

        public virtual ICollection<MailboxModel> Mailboxes {
            get { return _mailboxes; }
            set { _mailboxes = value; }
        }
    }
}