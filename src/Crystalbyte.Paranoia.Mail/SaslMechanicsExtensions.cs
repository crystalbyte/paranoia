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
using System.Linq;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    public static class SaslMechanicsExtensions {
        public static SaslMechanics GetSafest(this SaslMechanics mechanics) {
            return Enum.GetValues(typeof (SaslMechanics))
                .Cast<SaslMechanics>()
                .OrderByDescending(x => x)
                .FirstOrDefault(x => mechanics.HasFlag(x));
        }

        public static SaslMechanics GetFastest(this SaslMechanics mechanics) {
            return Enum.GetValues(typeof (SaslMechanics))
                .Cast<SaslMechanics>()
                .OrderBy(x => x)
                .FirstOrDefault(x => mechanics.HasFlag(x));
        }
    }
}