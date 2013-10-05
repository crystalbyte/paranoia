using System.Composition;
using Crystalbyte.Paranoia.Models;

namespace Crystalbyte.Paranoia.Contexts.Factories {
    [Export, Shared]
    public sealed class AccountContextFactory {

        [Import]
        public MessageContextFactory MessageContextFactory { get; set; }

        public AccountContext Create(Account account) {
            return new AccountContext(account) { MessageContextFactory = MessageContextFactory };
        }
    }
}
