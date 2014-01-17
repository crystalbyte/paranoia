#region Using directives

using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

#endregion

namespace Crystalbyte.Paranoia.Messaging {
    public sealed class SmtpSession : IDisposable {
        private readonly SmtpAuthenticator _authenticator;

        internal SmtpSession(SmtpAuthenticator authenticator) {
            _authenticator = authenticator;
        }

        #region Implementation of IDisposable

        public void Dispose() {
            _authenticator.Dispose();
        }

        #endregion
    }
}