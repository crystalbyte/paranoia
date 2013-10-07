using Crystalbyte.Paranoia.Messaging;
using System.Composition;

namespace Crystalbyte.Paranoia.Contexts.Factories {
    [Export, Shared]
    public sealed class ImapEnvelopeContextFactory {
        public ImapEnvelopeContext Create(Envelope envelope, ImapAccountContext account) {
            return new ImapEnvelopeContext(account, envelope);
        }
    }
}
