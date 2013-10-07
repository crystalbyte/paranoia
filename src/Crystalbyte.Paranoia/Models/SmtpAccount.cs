using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Models {
    public sealed class SmtpAccount {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public short Port { get; set; }
    }
}
