using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Crystalbyte.Paranoia.Messaging {
    public sealed class Envelope {

        const string FetchPattern = "(RFC822.SIZE [0-9]+)|((INTERNALDATE \".+?\"))|(FLAGS \\(.*?\\))";
        private static readonly Regex FetchRegex = new Regex(FetchPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        const string EnvelopePattern = "\\(\\(.+?\\)\\)|NIL|\"\"|<.+?>|\".+?\"";
        private static readonly Regex EnvelopeRegex = new Regex(EnvelopePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        const string DatePattern = @"\d{2}-\w{3}-\d{4} \d{2}:\d{2}:\d{2} (\+|\-)\d{4}";
        private static readonly Regex DateRegex = new Regex(DatePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly List<string> _flags;
        private List<MailContact> _from;

        public Envelope() {
            _flags = new List<string>();
            _from   = new List<MailContact>();
        }

        public long Uid { get; internal set; }
        public DateTime? InternalDate { get; internal set; }
        public string Subject { get; internal set; }
        public long Size { get; internal set; }
        public string MessageId { get; internal set; }
        public string InReplyTo { get; internal set; }

        public IEnumerable<MailContact> From {
            get { return _from; }
        }

        public IEnumerable<MailContact> Sender {
            get { return _from; }
        }

        public IEnumerable<MailContact> To {
            get { return _from; }
        }

        public IEnumerable<string> Flags {
            get { return _flags; }
        }

        public static Envelope Parse(string text) {
            var envelope = new Envelope();
            var matches = FetchRegex.Matches(text);
            foreach (Match match in matches) {
                if (match.Value.StartsWith("RFC822.SIZE", StringComparison.InvariantCultureIgnoreCase)) {
                    envelope.Size = long.Parse(match.Value.Split(' ').Last());
                    continue;
                }
                if (match.Value.StartsWith("INTERNALDATE", StringComparison.InvariantCultureIgnoreCase)) {
                    var date = DateRegex.Match(match.Value).Value;
                    envelope.InternalDate = DateTime.Parse(date);
                    continue;
                }
                if (match.Value.StartsWith("FLAGS", StringComparison.InvariantCultureIgnoreCase)) {
                                 const string pattern = @"\\[A-za-z0-9\*]+";
                    var flags = Regex.Matches(match.Value, pattern, RegexOptions.IgnoreCase)
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
            envelope.InReplyTo = matches[8].Value;
            envelope.MessageId = matches[9].Value;

            return envelope;
        }

        private void AddContactsToCc(IEnumerable<MailContact> contacts) {
            
        }

        private void AddContactsToBcc(IEnumerable<MailContact> contacts) {

        }

        private static IEnumerable<MailContact> ParseContacts(string value) {
            return new List<MailContact>();
        }

        private void AddContactsToFrom(IEnumerable<MailContact> contacts) {
            
        }

        private void AddContactsToSender(IEnumerable<MailContact> contacts) {

        }

        private void AddContactsToRecipients(IEnumerable<MailContact> contacts) {

        }

        private void AddContactsToReplyTo(IEnumerable<MailContact> contacts) {

        }


        private void AddFlags(IEnumerable<string> flags) {
            _flags.AddRange(flags);
        }
    }
}