using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    interface IMessageSource {
        Task<IEnumerable<MailMessageContext>> GetMessagesAsync();
    }
}
