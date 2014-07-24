using System;
using System.Text.RegularExpressions;

namespace Crystalbyte.Paranoia.UI {
    public sealed class MailAddressTokenMatcher : ITokenMatcher {

        private const string MailAddressPattern = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+(?:[A-Z]{2}|com|org|net|edu|gov|mil|biz|info|mobi|name|aero|asia|jobs|museum)\b";
        public bool IsMatch(string value) {
            return Regex.IsMatch(value, MailAddressPattern,
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }
    }
}