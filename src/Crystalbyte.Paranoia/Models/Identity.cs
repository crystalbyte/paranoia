using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Models {
    public sealed class Identity {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PublicKey { get; set; }
        public SecureString PrivateKey { get; set; }
    }
}
