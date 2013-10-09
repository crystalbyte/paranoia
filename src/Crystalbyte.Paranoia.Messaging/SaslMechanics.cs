#region Using directives

using System;

#endregion

namespace Crystalbyte.Paranoia.Messaging {
    /// <summary>
    ///   An enumeration of all supported SASL mechanics.
    /// </summary>
    [Flags]
    public enum SaslMechanics {
        /// <summary>
        ///   Plain authentication send username and password not encoded through the net.
        ///   Plain uses no encryption and is considered unsafe when used outside a TLS/SSL session.
        /// </summary>
        Login = 1,

        /// <summary>
        ///   Login is similar to plain, only the technical implementation differs.
        ///   Login uses no encryption and is considered unsafe when used outside a TLS/SSL session.
        /// </summary>
        Plain = 2,

        //Ntlm = 4,
        /// <summary>
        ///   Ntlm(SPA) is a proprietary authentication system developed by Microsoft.
        ///   Ntlm(SPA) is encrypted but considered obsolete. It is only included to support legacy servers that require this protocol for authentication. 
        ///   It should be only used outside a TLS/SSL if no better encryption protocol is supported by the server.
        /// </summary>
        /// <summary>
        ///   Cram-MD5 uses a server challenge system and MD5 hashing for authentication.
        ///   Cram-MD5 is encrypted but considered obsolete. It should be only used outside a TLS/SSL
        ///   if no better encryption protocol is supported by the server.
        /// </summary>
        CramMd5 = 8
    }
}