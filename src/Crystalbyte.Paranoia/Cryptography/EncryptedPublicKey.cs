using Crystalbyte.Paranoia.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    internal sealed class EncryptedPublicKey {
        
        public byte[] EncryptedSecret { get; set; }
        public byte[] Nonce { get; set; }
        public byte[] Checksum { get; set; }
        public MailContact Contact { get; set; }

        public string ToMimeHeader() {
            var secret = Convert.ToBase64String(EncryptedSecret);
            var nonce = Convert.ToBase64String(Nonce);
            var checksum = Convert.ToBase64String(Checksum);
            return string.Format("s={0}; n={1}; c={2}", secret, nonce, checksum);
        }
    }
}
