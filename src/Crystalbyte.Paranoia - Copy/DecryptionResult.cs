using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    public sealed class DecryptionResult {
        public bool IsSuccessful { get; set; }
        public byte[] Bytes { get; set; }
    }
}
