using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Models {
    public class MailboxFlag {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Mailbox")]
        public int MailboxId { get; set; }
        [Required]
        [StringLength(32)]
        public string Name { get; set; }

        public Mailbox Mailbox { get; set; }
    }
}
