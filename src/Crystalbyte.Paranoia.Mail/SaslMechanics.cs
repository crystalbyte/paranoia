#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia.Mail
// 
// Crystalbyte.Paranoia.Mail is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using Directives

using System;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    /// <summary>
    ///     An enumeration of all supported SASL mechanics.
    /// </summary>
    [Flags]
    public enum SaslMechanics {
        /// <summary>
        ///     Plain authentication send username and password not encoded through the net.
        ///     Plain uses no encryption and is considered unsafe when used outside a TLS/SSL session.
        /// </summary>
        Login = 1,

        /// <summary>
        ///     Login is similar to plain, only the technical implementation differs.
        ///     Login uses no encryption and is considered unsafe when used outside a TLS/SSL session.
        /// </summary>
        Plain = 2,

        //Ntlm = 4,
        /// <summary>
        ///     Ntlm(SPA) is a proprietary authentication system developed by Microsoft.
        ///     Ntlm(SPA) is encrypted but considered obsolete. It is only included to support legacy servers that require this
        ///     protocol for authentication.
        ///     It should be only used outside a TLS/SSL if no better encryption protocol is supported by the server.
        /// </summary>
        /// <summary>
        ///     Cram-MD5 uses a server challenge system and MD5 hashing for authentication.
        ///     Cram-MD5 is encrypted but considered obsolete. It should be only used outside a TLS/SSL
        ///     if no better encryption protocol is supported by the server.
        /// </summary>
        CramMd5 = 8
    }
}