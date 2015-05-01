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
        private ICollection<MailMessage> _messages;

        public Mailbox() {
            _messages = new List<MailMessage>();
        }

        [Key]
        [Index]
        [Column("id")]
        public Int64 Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("delimiter")]
        public string Delimiter { get; set; }

        [Column("flags")]
        public string Flags { get; set; }

        [Column("not_seen_count")]
        public int NotSeenCount { get; set; }

        [Column("is_subscribed")]
        public bool IsSubscribed { get; set; }

        [Column("account_id")]
        [ForeignKey("Account")]
        public Int64 AccountId { get; set; }

        public virtual MailAccount Account { get; set; }

        public virtual ICollection<MailMessage> Messages {
            get { return _messages; }
            set { _messages = value; }
        }
    }
}