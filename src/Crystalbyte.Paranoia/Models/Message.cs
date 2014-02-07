using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Models {
    public class Message {
        public int Id { get; set; }
        public int MailboxId { get; set; }
        public string Subject { get; set; }
    }
}
