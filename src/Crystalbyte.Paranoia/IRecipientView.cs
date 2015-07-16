using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;

namespace Crystalbyte.Paranoia {
    internal interface IRecipientView {
        IEnumerable<MailCompositionAddress> GetAddresses();
        void SetAddresses(MailCompositionAddress[] addresses);
    }
}
