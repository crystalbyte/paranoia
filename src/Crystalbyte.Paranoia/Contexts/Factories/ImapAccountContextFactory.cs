using System.Composition;
using Crystalbyte.Paranoia.Models;

namespace Crystalbyte.Paranoia.Contexts.Factories {
    [Export, Shared]
    public sealed class ImapAccountContextFactory {

        [Import]
        public ImapEnvelopeContextFactory ImapEnvelopeContextFactory { get; set; }

        public ImapAccountContext Create(ImapAccount account) {
            return new ImapAccountContext(account) { MessageContextFactory = ImapEnvelopeContextFactory };
        }
    }
}
