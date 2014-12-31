using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    public interface IMailboxCreator {
        Task CreateMailboxAsync(string name);
        bool CanHaveChildren { get; }
        bool CheckForValidName(string name);
    }
}
