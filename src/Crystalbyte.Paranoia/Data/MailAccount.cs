#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using Directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crystalbyte.Paranoia.Mail;

#endregion

namespace Crystalbyte.Paranoia.Data {
    [Table("mail_account")]
    internal class MailAccount {
        private ICollection<Mailbox> _mailboxes;
        private ICollection<MailComposition> _compositions;

        public MailAccount() {
            _mailboxes = new Collection<Mailbox>();
            _compositions = new Collection<MailComposition>();
        }

        [Key]
        [Index]
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

        [Column("is_default_time")]
        public DateTime IsDefaultTime { get; set; }

        [Column("imap_security")]
        public SecurityProtocol ImapSecurity { get; set; }

        [Column("smtp_host")]
        public string SmtpHost { get; set; }

        [Column("smtp_port")]
        public Int16 SmtpPort { get; set; }

        [Column("signature_path")]
        public string SignaturePath { get; set; }

        [Column("smtp_username")]
        public string SmtpUsername { get; set; }

        [Column("smtp_password")]
        public string SmtpPassword { get; set; }

        [Column("smtp_security")]
        public SecurityProtocol SmtpSecurity { get; set; }

        [Column("smtp_require_auth")]
        public bool SmtpRequiresAuthentication { get; set; }

        [Column("use_imap_credentials")]
        public bool UseImapCredentialsForSmtp { get; set; }

        [Column("store_sent_copies")]
        public bool StoreCopiesOfSentMessages { get; set; }

        [Column("sent_mailbox_name")]
        public string SentMailboxName { get; set; }

        [Column("draft_mailbox_name")]
        public string DraftMailboxName { get; set; }

        [Column("trash_mailbox_name")]
        public string TrashMailboxName { get; set; }

        [Column("junk_mailbox_name")]
        public string JunkMailboxName { get; set; }

        [Column("set_as_default_time")]
        public DateTime SetAsDefaultTime { get; set; }

        public virtual ICollection<Mailbox> Mailboxes {
            get { return _mailboxes; }
            set { _mailboxes = value; }
        }

        public virtual ICollection<MailComposition> Compositions {
            get { return _compositions; }
            set { _compositions = value; }
        }
    }
}