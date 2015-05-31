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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crystalbyte.Paranoia.Data.SQLite;

#endregion

namespace Crystalbyte.Paranoia.Data {
    
    [Table("mail_message")]
    internal class MailMessage {

        private List<MailMessageFlag> _flags;
        private List<MailAddress> _addresses;
        private List<MailAttachment> _attachments;

        public MailMessage() {
            _flags = new List<MailMessageFlag>();
            _addresses = new List<MailAddress>();
            _attachments = new List<MailAttachment>();
        }

        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        [Index]
        [Column("uid")]
        public Int64 Uid { get; set; }

        [Column("size")]
        public Int64 Size { get; set; }

        [Column("mime")]
        public byte[] Mime { get; set; }

        [Column("subject")]
        [Collate(CollatingSequence.NoCase)]
        public string Subject { get; set; }
        
        [Column("date")]
        public DateTime Date { get; set; }

        [Column("message_id")]
        public string MessageId { get; set; }

        [Index]
        [Column("mailbox_id")]
        public Int64 MailboxId { get; set; }

        [ForeignKey("MailboxId")]
        public Mailbox Mailbox { get; set; }

        public virtual List<MailMessageFlag> Flags {
            get { return _flags; }
            set { _flags = value; }
        }

        public virtual List<MailAddress> Addresses {
            get { return _addresses; }
            set { _addresses = value; }
        }

        public virtual List<MailAttachment> Attachments {
            get { return _attachments; }
            set { _attachments = value; }
        }
    }
}