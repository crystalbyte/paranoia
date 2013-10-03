using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Messaging {
    internal static class SequenceIdentifier {
        private static long _current = 0;
        public static string CreateNext() {
            return string.Format("p0{0}", _current++);
        }
    }
}
