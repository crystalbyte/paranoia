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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crystalbyte.Paranoia.Data.SQLite;

#endregion

namespace Crystalbyte.Paranoia.Data {
    [Table("public_key")]
    internal class PublicKey {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Index]
        [Column("contact_id")]
        public long ContactId { get; set; }

        [Column("device")]
        public string Device { get; set; }

        [Column("bytes")]
        [Collate(CollatingSequence.Binary)]
        public byte[] Bytes { get; set; }

        [Column("date")]
        [Default(DatabaseFunction.CurrentTimestamp)]
        public DateTime Date { get; set; }

        [ForeignKey("ContactId")]

        public MailContact Contact { get; set; }
    }
}