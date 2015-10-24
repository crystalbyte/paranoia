using Crystalbyte.Paranoia.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    internal sealed class PublicKeyEncryptionEntry {
        public MailContact Contact { get; set; }
        public byte[] EncryptedSecret { get; set; }
        public byte[] Nonce { get; set; }

        public override string ToString() {
            var secret = Convert.ToBase64String(EncryptedSecret);
            var nonce = Convert.ToBase64String(Nonce);
            return string.Format("v=1; s={1}; n={2}", secret, nonce);
        }
    }
}
