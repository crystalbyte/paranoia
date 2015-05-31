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

        public string Fullname {
            get; internal set;
        }

        public char Delimiter {
            get; internal set;
        }

        public IEnumerable<string> Flags {
            get; set;
        }

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
            var info = new ImapMailboxInfo(name, parts[2].TrimQuotes().ToCharArray().First())
            {
                Flags = Regex.Match(parts[1], @"\(.*\)").Value.Trim(new[] {'(', ')'}).Split(' ')
            };
            return info;
        }
    }
}