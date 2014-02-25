using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Models {
    public class MailFlag {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Mail")]
        public int MailId { get; set; }
        [Required]
        [StringLength(32)]
        public string Name { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }

        public virtual Mail Mail { get; set; }
    }
}
