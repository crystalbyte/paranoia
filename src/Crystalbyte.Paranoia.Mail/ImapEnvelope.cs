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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    /// <summary>
    ///     Defines the envelope for a mime encoded message.
    ///     For header information visit: http://tools.ietf.org/html/rfc3501#section-7.4.2
    /// </summary>
    [DebuggerDisplay("Subject = {Subject}")]
    public sealed class ImapEnvelope {
        private readonly List<string> _flags;
        private readonly List<MailAddress> _sender;
        private readonly List<MailAddress> _from;
        private readonly List<MailAddress> _to;
        private readonly List<MailAddress> _cc;
        private readonly List<MailAddress> _bcc;
        private readonly List<KeyValuePair<string, string>> _headers;

        private const string HostRegex = @"^(?=.{1,255}$)[0-9A-Za-z](?:(?:[0-9A-Za-z]|-){0,61}[0-9A-Za-z])?(?:\.[0-9A-Za-z](?:(?:[0-9A-Za-z]|-){0,61}[0-9A-Za-z])?)*\.?$";

        private const string FetchMetaPattern =
            "(RFC822.SIZE [0-9]+)|((INTERNALDATE \".+?\"))|(FLAGS \\(.*?\\))|UID \\d+";

        private static readonly Regex FetchMetaRegex = new Regex(FetchMetaPattern,
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        private const string EnvelopePattern = "\\(\\(.+?\\)\\)|NIL|\"\"|<.+?>|\".+?\"";

        private static readonly Regex EnvelopeRegex = new Regex(EnvelopePattern,
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private const string DatePattern = @"(\d{1}|\d{2})-\w{3}-\d{4} \d{2}:\d{2}:\d{2} (\+|\-)\d{4}";
        private static readonly Regex DateRegex = new Regex(DatePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public ImapEnvelope() {
            _flags = new List<string>();
            _from = new List<MailAddress>();
            _to = new List<MailAddress>();
            _bcc = new List<MailAddress>();
            _cc = new List<MailAddress>();
            _sender = new List<MailAddress>();
            _headers = new List<KeyValuePair<string, string>>();
        }

        public long Uid { get; internal set; }

        public DateTime? InternalDate { get; internal set; }

        public string Subject { get; internal set; }

        public long Size { get; internal set; }

        public string MessageId { get; internal set; }

        public string InReplyTo { get; internal set; }

        public IList<KeyValuePair<string, string>> Headers {
            get { return _headers; }
        }

        /// <summary>
        ///     Mailbox for replies to message.
        /// </summary>
        public MailAddress ReplyTo { get; internal set; }

        /// <summary>
        ///     Mailbox of message author.
        /// </summary>
        public IEnumerable<MailAddress> From {
            get { return _from; }
        }

        public IEnumerable<MailAddress> Cc {
            get { return _cc; }
        }

        public IEnumerable<MailAddress> Bcc {
            get { return _bcc; }
        }

        /// <summary>
        ///     Mailbox of message sender.
        /// </summary>
        public IEnumerable<MailAddress> Sender {
            get { return _from; }
        }

        /// <summary>
        ///     Primary recipient mailbox.
        /// </summary>
        public IEnumerable<MailAddress> To {
            get { return _from; }
        }

        public IEnumerable<string> Flags {
            get { return _flags; }
        }

        public static ImapEnvelope Parse(string text) {
            // Escape nested quotes.
            // TODO: Should be included in the regex.
            text = text.Replace("\\\"", "%%%");

            var envelope = new ImapEnvelope();
            var matches = FetchMetaRegex.Matches(text);
            foreach (Match match in matches) {
                if (match.Value.StartsWith("RFC822.SIZE", StringComparison.InvariantCultureIgnoreCase)) {
                    envelope.Size = long.Parse(match.Value.Split(' ').Last());
                    continue;
                }
                if (match.Value.StartsWith("UID", StringComparison.InvariantCultureIgnoreCase)) {
                    envelope.Uid = long.Parse(match.Value.Split(' ').Last());
                    continue;
                }
                if (match.Value.StartsWith("INTERNALDATE", StringComparison.InvariantCultureIgnoreCase)) {
                    var date = DateRegex.Match(match.Value).Value;
                    envelope.InternalDate = DateTime.Parse(date);
                    continue;
                }
                if (match.Value.StartsWith("FLAGS", StringComparison.InvariantCultureIgnoreCase)) {
                    const string pattern = @"\\[A-za-z0-9\*]+";
                    var flags = Regex.Matches(match.Value, pattern, RegexOptions.CultureInvariant)
                        .Cast<Match>()
                        .Select(x => x.Value);

                    envelope.AddFlags(flags);
                }
            }

            String[] t = { "ENVELOPE" };
            var temp = text.Split(t, StringSplitOptions.None);
            matches = EnvelopeRegex.Matches(temp[1]);
            envelope.Subject = TransferEncoder.Decode(matches[1].Value)
                .TrimQuotes()
                .Replace("%%%", "\"");

            envelope.AddContactsToFrom(ParseContacts(matches[2].Value));
            envelope.AddContactsToSender(ParseContacts(matches[3].Value));
            envelope.ReplyTo = ParseContacts(matches[4].Value).FirstOrDefault();
            envelope.AddContactsToRecipients(ParseContacts(matches[5].Value));
            envelope.AddContactsToCc(ParseContacts(matches[6].Value));
            envelope.AddContactsToBcc(ParseContacts(matches[7].Value));
            envelope.InReplyTo = matches[8].Value.TrimQuotes().TrimNil();
            envelope.MessageId = matches[9].Value.TrimQuotes().TrimNil();

            return envelope;
        }

        private static IEnumerable<MailAddress> ParseContacts(string value) {
            var trimmed = value.TrimAny(1).TrimQuotes();
            var entries = Regex.Matches(trimmed, "\\\"\\\"|\\\".+?\\\"|NIL");
            var contacts = entries
                .OfType<Match>()
                .Select(x => x.Value.Trim(new[] { '"', '\\' }))
                .Bundle(4).ToArray()
                .ToArray();

            return from contact in contacts 
                   select contact.ToArray() 
                   into items 
                   let name = TransferEncoder.Decode(items[0]) 
                   let host = items[3] 
                   let address = string.Format("{0}@{1}", items[2], items[3]) 
                   let isValid = Regex.Match(address, RegexPatterns.EmailAddressPattern).Success 
                   where isValid 
                   where !string.IsNullOrEmpty(items[3]) && Regex.IsMatch(host, HostRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled) 
                   select new MailAddress(address, name);
        }

        private void AddContactsToCc(IEnumerable<MailAddress> contacts) {
            _cc.AddRange(contacts);
        }

        private void AddContactsToBcc(IEnumerable<MailAddress> contacts) {
            _bcc.AddRange(contacts);
        }

        private void AddContactsToFrom(IEnumerable<MailAddress> contacts) {
            _from.AddRange(contacts);
        }

        private void AddContactsToSender(IEnumerable<MailAddress> contacts) {
            _sender.AddRange(contacts);
        }

        private void AddContactsToRecipients(IEnumerable<MailAddress> contacts) {
            _to.AddRange(contacts);
        }

        private void AddFlags(IEnumerable<string> flags) {
            _flags.AddRange(flags);
        }
    }
}