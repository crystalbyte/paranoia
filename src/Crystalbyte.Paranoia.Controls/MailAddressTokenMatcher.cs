#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia.Controls
// 
// Crystalbyte.Paranoia.Controls is free software: you can redistribute it and/or modify
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

using System.Collections.Generic;
using System.Text.RegularExpressions;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public sealed class MailAddressTokenMatcher : ITokenMatcher {
        #region Private Fields

        private readonly List<char> _triggerSymbols;

        private const string MailAddressPattern =
            @"[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?\.)+(?:[A-Zaa-z]{2}|com|org|net|edu|gov|mil|biz|info|mobi|name|aero|asia|jobs|museum)\b";

        #endregion

        #region Construction

        public MailAddressTokenMatcher() {
            _triggerSymbols = new List<char> {';', ' '};
        }

        #endregion

        public bool TryMatch(string value, out string match) {
            var pattern = string.Format("{0}[{1}]", MailAddressPattern, string.Join("|", TriggerSymbols));
            var result = Regex.Match(value, pattern,
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

            match = result.Value.TrimEnd(TriggerSymbols.ToArray());
            return result.Success;
        }

        public List<char> TriggerSymbols {
            get { return _triggerSymbols; }
        }
    }
}