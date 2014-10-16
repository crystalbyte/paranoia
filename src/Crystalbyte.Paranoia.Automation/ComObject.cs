using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Automation {
    public abstract class ComObject {
        private readonly IComServer _server;

        protected ComObject(IComServer server) {
            _server = server;
            _server.IncrementObjectCount();
        }

        ~ComObject() {
            _server.DecrementObjectCount();
        }
    }
}
