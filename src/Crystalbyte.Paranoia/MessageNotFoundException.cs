using System;
using System.Runtime.Serialization;

namespace Crystalbyte.Paranoia {
    [Serializable]
    public class MessageNotFoundException : Exception {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public MessageNotFoundException() { }

        public MessageNotFoundException(long uid)
            : this(string.Format("Message with UID {0} not found.", uid)) { }

        public MessageNotFoundException(string message) : base(message) { }
        public MessageNotFoundException(string message, Exception inner) : base(message, inner) { }

        protected MessageNotFoundException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) { }
    }
}
