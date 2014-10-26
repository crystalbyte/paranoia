#region Using directives

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

        [Column("is_trusted")]
        public bool IsTrusted { get; set; }

        [Column("is_blocked")]
        public bool IsBlocked { get; set; }

        public virtual ICollection<PublicKeyModel> Keys {
            get { return _keys; }
            set { _keys = value; }
        }
    }
}