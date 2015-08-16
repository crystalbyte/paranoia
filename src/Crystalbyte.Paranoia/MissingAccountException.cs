using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {

    [Serializable]
    public class MissingAccountException : Exception {
        public MissingAccountException() { }
        public MissingAccountException(string message) : base(message) { }
        public MissingAccountException(string message, Exception inner) : base(message, inner) { }
        protected MissingAccountException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
