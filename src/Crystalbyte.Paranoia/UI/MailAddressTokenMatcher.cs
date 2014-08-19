#region Using directives

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