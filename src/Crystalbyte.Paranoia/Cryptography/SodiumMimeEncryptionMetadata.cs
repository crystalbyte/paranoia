using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {

    internal sealed class SodiumMimeEncryptionMetadata {

        #region Private Fields

        private List<EncryptedSecret> _entries;

        #endregion

        #region Construction

        public SodiumMimeEncryptionMetadata() {
            _entries = new List<EncryptedSecret>();
        }

        #endregion

        #region Properties

        public string Version { get; set; }

        public byte[] Secret { get; set; }

        public byte[] EncryptedMessage { get; set; }

        public byte[] Nonce { get; set; }

        public List<EncryptedSecret> Entries {
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
