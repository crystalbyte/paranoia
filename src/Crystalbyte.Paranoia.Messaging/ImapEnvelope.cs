#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

#endregion

namespace Crystalbyte.Paranoia.Messaging {
    /// <summary>
    /// Defines the envelope for a mime encoded message.
    /// For header information visit: https://tools.ietf.org/html/rfc4021
    /// </summary>
    [DebuggerDisplay("Subject = {Subject}")]
    public sealed class ImapEnvelope {
        private readonly List<string> _flags;
        private readonly List<MailAddress> _sender;
        private readonly List<MailAddress> _from;
        private readonly List<MailAddress> _to;
        private readonly List<MailAddress> _cc;
        private readonly List<MailAddress> _bcc;

        private const string FetchHeadersPattern = "RFC822\\.HEADER[\\s]*{[0-9]+}.+\\n\\n";

        private const string FetchMetaPattern = "(RFC822.SIZE [0-9]+)|((INTERNALDATE \".+?\"))|(FLAGS \\(.*?\\))|UID \\d+";
        private static readonly Regex FetchMetaRegex = new Regex(FetchMetaPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private const string EnvelopePattern = "\\(\\(.+?\\)\\)|NIL|\"\"|<.+?>|\".+?\"";
        private static readonly Regex EnvelopeRegex = new Regex(EnvelopePattern, RegexOptions.IgnoreCase);

        private const string DatePattern = @"\d{2}-\w{3}-\d{4} \d{2}:\d{2}:\d{2} (\+|\-)\d{4}";
        private static readonly Regex DateRegex = new Regex(DatePattern, RegexOptions.IgnoreCase);

        public ImapEnvelope() {
            _flags = new List<string>();
            _from = new List<MailAddress>();
            _to = new List<MailAddress>();
            _bcc = new List<MailAddress>();
            _cc = new List<MailAddress>();
            _sender = new List<MailAddress>();
        }

        public long Uid { get; internal set; }
        public DateTime? InternalDate { get; internal set; }
        public string Subject { get; internal set; }
        public long Size { get; internal set; }
        public string MessageId { get; internal set; }
        public string InReplyTo { get; internal set; }

        /// <summary>
        /// Mailbox for replies to message.
        /// </summary>
        public MailAddress ReplyTo { get; internal set; }

        /// <summary>
        /// Mailbox of message author.
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
        ///  Mailbox of message sender.
        /// </summary>
        public IEnumerable<MailAddress> Sender {
            get { return _from; }
        }

        /// <summary>
        /// Primary recipient mailbox.
        /// </summary>
        public IEnumerable<MailAddress> To {
            get { return _from; }
        }

        public IEnumerable<string> Flags {
            get { return _flags; }
        }

        public static ImapEnvelope Parse(string text) {
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

            matches = EnvelopeRegex.Matches(text);
            envelope.Subject = TransferEncoder.Decode(matches[1].Value).TrimQuotes();
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
            var contacts = Regex.Matches(trimmed, @"\(.+?\)");
            foreach (var items in from Match contact in contacts select Regex.Matches(contact.Value, "\".+?\"|NIL")) {
                Debug.Assert(items.Count == 4);
                var name = items[0].Value.TrimQuotes();
                var address = string.Format("{0}@{1}", items[2].Value.TrimQuotes(), items[3].Value.TrimQuotes());
                yield return new MailAddress(address, name);
            }
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