using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {

    internal sealed class SodiumEncryptionMetadata {

        #region Private Fields

        private List<EncryptedPublicKey> _entries;

        #endregion

        #region Construction

        public SodiumEncryptionMetadata() {
            _entries = new List<EncryptedPublicKey>();
        }

        #endregion

        #region Properties

        public string Version { get; set; }

        public byte[] Secret { get; set; }

        public byte[] EncryptedMessage { get; set; }

        public byte[] Nonce { get; set; }

        public List<EncryptedPublicKey> Entries {
            get { return _entries; }
        }

        #endregion

        #region Methods

        public string ToMimeHeader() {
            return string.Format("v={0}; n={1}", Version, Nonce);
        }

        #endregion
    }
}
