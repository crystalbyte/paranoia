using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    internal sealed class MimeEncryptionResult {

        #region Private Fields

        private List<PublicKeyEncryptionEntry> _entries;

        #endregion

        #region Construction

        public MimeEncryptionResult() {
            _entries = new List<PublicKeyEncryptionEntry>();
        }

        #endregion

        #region Properties

        public byte[] Secret { get; set; }

        public byte[] EncryptedMessage { get; set; }

        public byte[] Nonce { get; set; }

        public List<PublicKeyEncryptionEntry> Entries {
            get { return _entries; }
        }

        #endregion

        #region Methods

        public string ToHeader() {
            return string.Format("v = 1; n = {0}", Nonce);
        }

        #endregion
    }
}
