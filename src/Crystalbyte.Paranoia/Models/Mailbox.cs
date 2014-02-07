using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Models {
    public class Mailbox {
        [Required]
        public int Id { get; set; }
        [Required]
        public int ImapAccountId { get; set; }
        public string Name { get; set; }
        public char Delimiter { get; set; }

        [ForeignKey("ImapAccountId")]
        public virtual List<MailboxFlag> Flags { get; set; }
    }
}
