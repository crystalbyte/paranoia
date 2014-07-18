using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Mail {
    public static class MailboxFlags {
        public static readonly string Seen = @"\seen";
        public static readonly string NoChildren = @"\nochildren";
        public static readonly string NoSelect = @"\noselect";
    }
}
