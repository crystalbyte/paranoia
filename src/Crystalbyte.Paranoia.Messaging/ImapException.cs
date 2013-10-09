#region Using directives

using System;
using System.Runtime.Serialization;

#endregion

namespace Crystalbyte.Paranoia.Messaging {
    [Serializable]
    public class ImapException : Exception {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public ImapException() {}
        public ImapException(string message) : base(message) {}
        public ImapException(string message, Exception inner) : base(message, inner) {}

        protected ImapException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {}
    }
}