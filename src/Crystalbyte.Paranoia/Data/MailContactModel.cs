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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace Crystalbyte.Paranoia.Data {
    [Table("mail_contact")]
    public class MailContactModel {
        private ICollection<PublicKeyModel> _keys;

        public MailContactModel() {
            _keys = new Collection<PublicKeyModel>();
        }

        [Key]
        [Column("id")]
        public Int64 Id { get; set; }

        [Column("name")]
        [Collate(CollatingSequence.NoCase)]
        public string Name { get; set; }

        [Column("address")]
        [Collate(CollatingSequence.NoCase)]
        public string Address { get; set; }

        [Column("is_external_content_allowed")]
        public bool IsExternalContentAllowed { get; set; }

        [Column("classification")]
        public ContactClassification Classification { get; set; }

        public virtual ICollection<PublicKeyModel> Keys {
            get { return _keys; }
            set { _keys = value; }
        }
    }
}