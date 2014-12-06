using System;
using System.Runtime.Serialization;

namespace Crystalbyte.Paranoia {
    [Serializable]
    public class ResourceNotFoundException : Exception {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public ResourceNotFoundException() {}
        public ResourceNotFoundException(string message) : base(message) {}
        public ResourceNotFoundException(string message, Exception inner) : base(message, inner) {}

        protected ResourceNotFoundException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {}
    }
}
