#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

#endregion

namespace Crystalbyte.Paranoia.Messaging {
    [DebuggerDisplay("Subject = {Subject}")]
    public sealed class ImapEnvelope {
        private readonly List<string> _flags;
        private readonly List<Contact> _sender;
        private readonly List<Contact> _from;
        private readonly List<Contact> _to;
        private readonly List<Contact> _cc;
        private readonly List<Contact> _bcc;
        private readonly List<Contact> _replyTo;

        private const string FetchPattern = "(RFC822.SIZE [0-9]+)|((INTERNALDATE \".+?\"))|(FLAGS \\(.*?\\))|UID \\d+";
        private static readonly Regex FetchRegex = new Regex(FetchPattern, RegexOptions.IgnoreCase);

        private const string EnvelopePattern = "\\(\\(.+?\\)\\)|NIL|\"\"|<.+?>|\".+?\"";
        private static readonly Regex EnvelopeRegex = new Regex(EnvelopePattern, RegexOptions.IgnoreCase);

        private const string DatePattern = @"\d{2}-\w{3}-\d{4} \d{2}:\d{2}:\d{2} (\+|\-)\d{4}";
        private static readonly Regex DateRegex = new Regex(DatePattern, RegexOptions.IgnoreCase);

        public ImapEnvelope() {
            _flags = new List<string>();
            _from = new List<Contact>();
            _to = new List<Contact>();
            _bcc = new List<Contact>();
            _cc = new List<Contact>();
            _sender = new List<Contact>();
            _replyTo = new List<Contact>();
        }

        public long Uid { get; internal set; }
        public DateTime? InternalDate { get; internal set; }
        public string Subject { get; internal set; }
        public long Size { get; internal set; }
        public string MessageId { get; internal set; }
        public string InReplyTo { get; internal set; }

        public IEnumerable<Contact> From {
            get { return _from; }
        }

        public IEnumerable<Contact> Sender {
            get { return _from; }
        }

        public IEnumerable<Contact> To {
            get { return _from; }
        }

        public IEnumerable<Contact> ReplyTo {
            get { return _replyTo; }
        }

        public IEnumerable<string> Flags {
            get { return _flags; }
        }

        public static ImapEnvelope Parse(string text) {
            var envelope = new ImapEnvelope();
            var matches = FetchRegex.Matches(text);
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
            envelope.AddContactsToReplyTo(ParseContacts(matches[4].Value));
            envelope.AddContactsToRecipients(ParseContacts(matches[5].Value));
            envelope.AddContactsToCc(ParseContacts(matches[6].Value));
            envelope.AddContactsToBcc(ParseContacts(matches[7].Value));
            envelope.InReplyTo = matches[8].Value.TrimQuotes().TrimNil();
            envelope.MessageId = matches[9].Value.TrimQuotes().TrimNil();
            return envelope;
        }

        private static IEnumerable<Contact> ParseContacts(string value) {
            var trimmed = value.TrimAny(1).TrimQuotes();
            var contacts = Regex.Matches(trimmed, @"\(.+?\)");
            foreach (var items in from Match contact in contacts select Regex.Matches(contact.Value, "\".+?\"|NIL")) {
                Debug.Assert(items.Count == 4);
                yield return new Contact {
                    Name = items[0].Value,
                    Address = string.Format("{0}@{1}", items[2].Value, items[3].Value)
                };
            }
        }

        private void AddContactsToCc(IEnumerable<Contact> contacts) {
            _cc.AddRange(contacts);
        }

        private void AddContactsToBcc(IEnumerable<Contact> contacts) {
            _bcc.AddRange(contacts);
        }

        private void AddContactsToFrom(IEnumerable<Contact> contacts) {
            _from.AddRange(contacts);
        }

        private void AddContactsToSender(IEnumerable<Contact> contacts) {
            _sender.AddRange(contacts);
        }

        private void AddContactsToRecipients(IEnumerable<Contact> contacts) {
            _to.AddRange(contacts);
        }

        private void AddContactsToReplyTo(IEnumerable<Contact> contacts) {
            _replyTo.AddRange(contacts);
        }

        private void AddFlags(IEnumerable<string> flags) {
            _flags.AddRange(flags);
        }
    }
}