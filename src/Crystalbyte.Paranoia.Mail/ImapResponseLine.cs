﻿#region Copyright Notice & Copying Permission

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

#endregion

namespace Crystalbyte.Paranoia.Mail {
    internal struct ImapResponseLine : IEquatable<ImapResponseLine> {
        public ImapResponseLine(string text)
            : this() {
            Text = text;
        }

        public string Text { get; private set; }

        public bool TerminatesCommand(string id) {
            return !string.IsNullOrWhiteSpace(Text) && Text.StartsWith(id, StringComparison.InvariantCultureIgnoreCase);
        }

        public bool IsBad {
            get { return !string.IsNullOrWhiteSpace(Text) && Text.ContainsIgnoreCase(" BAD "); }
        }

        public bool IsNo {
            get { return !string.IsNullOrWhiteSpace(Text) && Text.ContainsIgnoreCase(" NO "); }
        }

        public bool IsOk {
            get { return !string.IsNullOrWhiteSpace(Text) && Text.ContainsIgnoreCase(" OK "); }
        }

        public bool IsUntagged {
            get {
                return !string.IsNullOrWhiteSpace(Text) &&
                       Text.StartsWith("*", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public bool IsUntaggedOk {
            get {
                return !string.IsNullOrWhiteSpace(Text) &&
                       Text.StartsWith("* OK", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public bool IsContinuationRequest {
            get {
                return !string.IsNullOrWhiteSpace(Text) &&
                       Text.StartsWith("+", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public bool IsMultiline {
            get {
                return !string.IsNullOrWhiteSpace(Text) &&
                       Text.StartsWith(" ", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        #region Implementation of IEquatable<ResponseLine>

        public bool Equals(ImapResponseLine other) {
            return Text == other.Text;
        }

        #endregion
    }
}