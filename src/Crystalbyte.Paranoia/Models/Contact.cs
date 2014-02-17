using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Models {
    public class Contact {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Identity")]
        public int IdentityId { get; set; }
        [StringLength(128)]
        public string Name { get; set; }
        [Required]
        [StringLength(256)]
        public string Address { get; set; }
        [Required]
        public ContactRequest ContactRequest { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }

        public virtual Identity Identity { get; set; }
    }
}
