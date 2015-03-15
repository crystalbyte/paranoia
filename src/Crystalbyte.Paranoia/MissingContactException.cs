using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    [Serializable]
    public class MissingContactException : System.Exception {
        public MissingContactException() { }
        public MissingContactException(string message) : base(message) { }
        public MissingContactException(string message, System.Exception inner) : base(message, inner) { }
        protected MissingContactException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
