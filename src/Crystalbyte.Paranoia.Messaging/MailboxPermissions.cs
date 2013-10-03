using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Messaging {
    [Flags]
    public enum MailboxPermissions {
        Read = 0,
        Write
    }
}
