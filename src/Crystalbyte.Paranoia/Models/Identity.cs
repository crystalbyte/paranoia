using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Models {
    public class Identity {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public string Address { get; set; }
        public virtual SmtpAccount SmtpAccount { get; set; }
        public virtual ImapAccount ImapAccount { get; set; }
        public virtual List<Contact> Contacts { get; set; }
    }
}
