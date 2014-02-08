﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Models {
    public class Mailbox {
        [Key]
        public int Id { get; set; }
        [ForeignKey("ImapAccount")]
        public int ImapAccountId { get; set; }
        [Required]
        [StringLength(256)]
        public string Name { get; set; }
        [Required]
        public char Delimiter { get; set; }

        public virtual ImapAccount ImapAccount { get; set; }
        public virtual List<MailboxFlag> Flags { get; set; }
    }
}
