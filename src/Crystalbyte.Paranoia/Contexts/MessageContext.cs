using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class MessageContext {
        private string _structure;
        public MessageContext(string structure) {
            _structure = structure;
        }

        public string Text { get; set; }
    }
}
