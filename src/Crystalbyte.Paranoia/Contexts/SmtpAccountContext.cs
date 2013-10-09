#region Using directives

using Crystalbyte.Paranoia.Models;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class SmtpAccountContext {
        private readonly SmtpAccount _account;

        public SmtpAccountContext(SmtpAccount account) {
            _account = account;
        }

        public string Host {
            get { return _account.Host; }
        }

        public int Port {
            get { return _account.Port; }
        }

        public string Username {
            get { return _account.Username; }
        }

        public string Password {
            get { return _account.Password; }
        }
    }
}