#region Using directives

using System;
using System.Security.Authentication;

#endregion

namespace Crystalbyte.Paranoia.Messaging {
    public sealed class EncryptionProtocolNegotiatedEventArgs : EventArgs {
        internal EncryptionProtocolNegotiatedEventArgs(SslProtocols protocol, int strength) {
            Protocols = protocol;
            Strength = strength;
        }

        public SslProtocols Protocols { get; private set; }
        public int Strength { get; private set; }
    }
}