using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    [Serializable]
    public class SignetMissingOrCorruptException : Exception {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public SignetMissingOrCorruptException() {}
        public SignetMissingOrCorruptException(string message) : base(message) {}
        public SignetMissingOrCorruptException(string message, Exception inner) : base(message, inner) {}

        protected SignetMissingOrCorruptException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {}
    }
}
