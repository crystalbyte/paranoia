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