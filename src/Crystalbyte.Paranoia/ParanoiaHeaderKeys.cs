#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
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

namespace Crystalbyte.Paranoia {
    internal sealed class ParanoiaHeaderKeys {
        public const string Nonce = "X-p4r4n014-Nonce";
        public const string Token = "X-p4r4n014-Token";
        public const string PublicKey = "X-p4r4n014-PublicKey";
        public const string PublicKeyHash = "X-p4r4n014-PublicKey-Hash";
        public const string Type = "X-p4r4n014-Type";
    }
}