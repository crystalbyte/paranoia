#region Using directives

using System.Composition;
using Crystalbyte.Paranoia.Models;

#endregion

namespace Crystalbyte.Paranoia.Contexts.Factories {
    [Export, Shared]
    public sealed class SmtpAccountContextFactory {
        public SmtpAccountContext Create(SmtpAccount account) {
            return new SmtpAccountContext(account);
        }
    }
}