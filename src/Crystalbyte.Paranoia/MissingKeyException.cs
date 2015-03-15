using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    [Serializable]
    public class MissingKeyException : Exception {
        public MissingKeyException() { }
        public MissingKeyException(string message) : base(message) { }
        public MissingKeyException(string message, Exception inner) : base(message, inner) { }
        protected MissingKeyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
