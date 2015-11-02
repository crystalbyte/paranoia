using Crystalbyte.Paranoia.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Cryptography {
    internal static class ByteCollectionExtensions {
        public static SodiumMimeDecryptionMetadata ToMimeDecryptionMetadata(this byte[] bytes) {
            var reader = new MailMessageReader(bytes);
            var part = reader.FindAllMessagePartsWithMediaType(MediaTypes.EncryptedMime);
            if (part.Count == 0) {
                return new SodiumMimeDecryptionMetadata() {
                    IsEncrypted = false
                };
            }

            throw new NotImplementedException();
        }
    }
}
