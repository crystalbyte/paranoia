using Crystalbyte.Paranoia.Messaging;
using System.Composition;

namespace Crystalbyte.Paranoia.Contexts.Factories {
    [Export, Shared]
    public sealed class MessageContextFactory {
        public MessageContext Create(Envelope envelope, AccountContext context) {
            return new MessageContext(envelope, context);
        }
    }
}
