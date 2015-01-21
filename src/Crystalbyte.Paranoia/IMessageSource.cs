using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    interface IMessageSource {
        bool IsLoadingMessages { get; set; }
        Task<IEnumerable<MailMessageContext>> GetMessagesAsync();
    }
}
