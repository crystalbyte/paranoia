#region Using directives

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    [DebuggerDisplay("Fullname = {Fullname}")]
    public sealed class ImapMailboxInfo {
        public ImapMailboxInfo(string fullname, char delimiter) {
            Fullname = fullname;
            Delimiter = delimiter;
        }

        public string Name {
            get { return Fullname.Split(Delimiter).Last(); }
        }

        public string Fullname { get; internal set; }
        public char Delimiter { get; internal set; }
        public IEnumerable<string> Flags { get; set; }

        public bool HasNoChildren {
            get { return Flags.Any(x => x.ContainsIgnoreCase(@"\hasnochildren")); }
        }

        public bool IsSelectable {
            get { return Flags.All(x => !x.ContainsIgnoreCase(@"\noselect")); }
        }

        public bool IsInbox {
            get { return Name.ToLower() == "inbox"; }
        }

        public bool IsGmailAll {
            get { return Flags.Contains(@"\All"); }
        }

        public bool IsGmailTrash {
            get { return Flags.Contains(@"\Trash"); }
        }

        public bool IsGmailSent {
            get { return Flags.Contains(@"\Sent"); }
        }

        public bool IsFlagged {
            get { return Flags.Contains(@"\Flagged"); }
        }

        public bool IsGmailJunk {
            get { return Flags.Contains(@"\Junk"); }
        }

        public bool IsGmailImportant {
            get { return Flags.Contains(@"\Important"); }
        }

        public bool IsGmailDraft {
            get { return Flags.Contains(@"\Drafts"); }
        }

        internal static ImapMailboxInfo Parse(ImapResponseLine line) {
            const string pattern = "\\(.*?\\)|\".+?\"|\\w+";
            var parts = Regex.Matches(line.Text, pattern)
                .OfType<Match>()
                .Select(x => x.Value)
                .ToArray();

            var name = ImapMailbox.DecodeName(parts[3].TrimQuotes());
            var info = new ImapMailboxInfo(name, parts[2].TrimQuotes().ToCharArray().First()) {
                Flags = Regex.Match(parts[1], @"\(.*\)").Value.Trim(new[] { '(', ')' }).Split(' ')
            };
            return info;
        }
    }
}