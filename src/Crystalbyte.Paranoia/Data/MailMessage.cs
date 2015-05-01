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
using Crystalbyte.Paranoia.Data.SQLite;

#endregion

namespace Crystalbyte.Paranoia.Data {
    
    [Table("mail_message")]
    [Virtual(ModuleType.Fts3)]
    internal class MailMessage {
        private ICollection<MimeMessage> _mimeMessages;

        public MailMessage() {
            _mimeMessages = new Collection<MimeMessage>();
        }

        [Key]
        [Index]
        [Column("id")]
        public Int64 Id { get; set; }

        [Column("uid")]
        public Int64 Uid { get; set; }

        [Column("size")]
        public Int64 Size { get; set; }

        [Column("flags")]
        [Collate(CollatingSequence.NoCase)]
        public string Flags { get; set; }

        [Column("subject")]
        [Collate(CollatingSequence.NoCase)]
        public string Subject { get; set; }

        [Index]
        [Column("has_attachments")]
        public bool HasAttachments { get; set; }

        [Column("entry_date")]
        public DateTime EntryDate { get; set; }

        [Column("thread_id")]
        public string ThreadId { get; set; }

        [Column("message_id")]
        public string MessageId { get; set; }

        [Column("from_name")]
        [Collate(CollatingSequence.NoCase)]
        public string FromName { get; set; }

        [Column("from_address")]
        [Collate(CollatingSequence.NoCase)]
        public string FromAddress { get; set; }

        [Column("mailbox_id")]
        [ForeignKey("Mailbox")]
        public Int64 MailboxId { get; set; }

        public Mailbox Mailbox { get; set; }

        public virtual ICollection<MimeMessage> MimeMessages {
            get { return _mimeMessages; }
            set { _mimeMessages = value; }
        }
    }
}