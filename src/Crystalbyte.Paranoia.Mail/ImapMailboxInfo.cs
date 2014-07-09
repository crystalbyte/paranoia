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

        public bool IsAll {
            get { return Flags.Contains(@"\All"); }
        }

        public bool IsTrash {
            get { return Flags.Contains(@"\Trash"); }
        }

        public bool IsSent {
            get { return Flags.Contains(@"\Sent"); }
        }

        public bool IsFlagged {
            get { return Flags.Contains(@"\Flagged"); }
        }

        public bool IsJunk {
            get { return Flags.Contains(@"\Junk"); }
        }

        public bool IsImportant {
            get { return Flags.Contains(@"\Important"); }
        }

        public bool IsDraft {
            get { return Flags.Contains(@"\Drafts"); }
        }

        internal static ImapMailboxInfo Parse(ImapResponseLine line) {
            var parts = Regex.Matches(line.Text, "\\(.*\\)|(\".*?\")").Cast<Match>().Select(x => x.Value).ToArray();
            var name = ImapMailbox.DecodeName(parts[2].TrimQuotes());
            var info = new ImapMailboxInfo(name, parts[1].TrimQuotes().ToCharArray().First())
            {
                Flags = Regex.Match(parts[0], @"\(.*\)").Value.Trim(new[] {'(', ')'}).Split(' ')
            };
            return info;
        }
    }
}