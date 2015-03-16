using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Cryptography {
    [Serializable]
    public class MessageDecryptionFailedException : Exception {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public MessageDecryptionFailedException() {}
        public MessageDecryptionFailedException(string message) : base(message) {}
        public MessageDecryptionFailedException(string message, Exception inner) : base(message, inner) {}

        protected MessageDecryptionFailedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {}
    }
}
