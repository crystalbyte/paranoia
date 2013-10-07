using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Models;

namespace Crystalbyte.Paranoia.Contexts.Factories {
    [Export, Shared]
    public sealed class SmtpAccountContextFactory {

        public SmtpAccountContext Create(SmtpAccount account) {
            return new SmtpAccountContext(account);
        }
    }
}
