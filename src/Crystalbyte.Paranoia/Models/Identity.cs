using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Models {
    public class Identity {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(128)]
        public string Name { get; set; }
        [Required]
        [StringLength(256)]
        public string Address { get; set; }
        public virtual SmtpAccount SmtpAccount { get; set; }
        public virtual ImapAccount ImapAccount { get; set; }
        public virtual List<Contact> Contacts { get; set; }
    }
}
