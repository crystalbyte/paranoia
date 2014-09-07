using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    interface IMessageSource {
        Task<IEnumerable<MailMessageContext>> GetMessagesAsync();
    }
}
