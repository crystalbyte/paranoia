using Crystalbyte.Paranoia.Data;
using System.Diagnostics;

namespace Crystalbyte.Paranoia {
    [DebuggerDisplay("Name = {Name}, MailAddress = {MailAddress}")]
    public sealed class MailAddressContext {
        private readonly MailAddress _address;

        internal MailAddressContext(MailAddress address) {
            _address = address;
        }

        public string Name {
            get { return _address.Name; }
        }

        public string Address {
            get { return _address.Address; }
        }

        public AddressRole Role {
            get { return _address.Role; }
        }
    }
}
