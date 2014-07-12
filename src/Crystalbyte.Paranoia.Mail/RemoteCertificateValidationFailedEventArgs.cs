#region Using directives

using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    public sealed class RemoteCertificateValidationFailedEventArgs : EventArgs {
        internal RemoteCertificateValidationFailedEventArgs(X509Certificate certificate, X509Chain chain,
            SslPolicyErrors error) {
            Chain = chain;
            Certificate = certificate;
            PolicyError = error;
            IsCanceled = true;
        }

        public X509Certificate Certificate { get; private set; }
        public SslPolicyErrors PolicyError { get; private set; }
        public X509Chain Chain { get; private set; }
        public bool IsCanceled { get; set; }
    }
}