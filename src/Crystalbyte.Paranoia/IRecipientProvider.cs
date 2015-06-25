using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    internal interface IRecipientProvider {
        IEnumerable<string> GetTo();
        IEnumerable<string> GetCc();
        IEnumerable<string> GetBcc();
    }
}
