using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    interface IMessageSource {
        bool IsLoadingMessages { get; }
        Task<IEnumerable<MailMessageContext>> GetMessagesAsync();
    }
}
