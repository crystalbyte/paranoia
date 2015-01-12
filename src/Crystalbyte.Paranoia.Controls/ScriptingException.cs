using System;
using System.Runtime.Serialization;

namespace Crystalbyte.Paranoia.UI {
    [Serializable]
    public class ScriptingException : Exception {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public ScriptingException() {}
        public ScriptingException(string message) : base(message) {}
        public ScriptingException(string message, Exception inner) : base(message, inner) {}

        protected ScriptingException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {}
    }
}
