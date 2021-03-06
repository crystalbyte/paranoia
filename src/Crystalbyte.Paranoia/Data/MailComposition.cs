﻿#region Copyright Notice & Copying Permission

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
    [Table("mail_composition")]
    internal class MailComposition {

        private List<MailCompositionAddress> _addresses;
        private List<MailCompositionAttachment> _attachments;

        public MailComposition() {
            _addresses = new List<MailCompositionAddress>();
            _attachments = new List<MailCompositionAttachment>();
        }

        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("account_id")]
        public long AccountId { get; set; }

        [Column("created")]
        [Default(DatabaseFunction.CurrentTimestamp)]
        public DateTime Created { get; set; }

        [Column("subject")]
        public string Subject { get; set; }

        [Column("last_change")]
        [Default(DatabaseFunction.CurrentTimestamp)]
        public DateTime LastChange { get; set; }

        [Column("content")]
        public string Content { get; set; }

        public virtual List<MailCompositionAddress> Addresses {
            get { return _addresses; }
            set { _addresses = value; }
        }

        public virtual List<MailCompositionAttachment> Attachments {
            get { return _attachments; }
            set { _attachments = value; }
        }
    }
}