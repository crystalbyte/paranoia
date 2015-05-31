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

#endregion

namespace Crystalbyte.Paranoia.Data {
    [Table("mailbox")]
    internal class Mailbox {
        private List<MailMessage> _messages;
        private List<MailboxFlag> _flags;

        public Mailbox() {
            _messages = new List<MailMessage>();
            _flags = new List<MailboxFlag>();
        }

        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("delimiter")]
        public string Delimiter { get; set; }

        [Column("is_subscribed")]
        public bool IsSubscribed { get; set; }

        [Column("is_local")]
        public bool IsLocal { get; set; }

        [Index]
        [Column("account_id")]
        public Int64 AccountId { get; set; }

        [ForeignKey("AccountId")]
        public MailAccount Account { get; set; }

        public virtual List<MailMessage> Messages {
            get { return _messages; }
            set { _messages = value; }
        }

        public virtual List<MailboxFlag> Flags {
            get { return _flags; }
            set { _flags = value; }
        }
    }
}