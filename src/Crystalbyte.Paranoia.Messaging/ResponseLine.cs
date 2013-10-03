﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Messaging {
    internal struct ResponseLine : IEquatable<ResponseLine> {

        public ResponseLine(string text)
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
            get { return !string.IsNullOrWhiteSpace(Text) && Text.StartsWith("*", StringComparison.InvariantCultureIgnoreCase); }
        }

        public bool IsUntaggedOk {
            get { return !string.IsNullOrWhiteSpace(Text) && Text.StartsWith("* OK", StringComparison.InvariantCultureIgnoreCase); }
        }

        public bool IsContinuationRequest {
            get { return !string.IsNullOrWhiteSpace(Text) && Text.StartsWith("+", StringComparison.InvariantCultureIgnoreCase); }
        }

        public bool IsMultiline {
            get { return !string.IsNullOrWhiteSpace(Text) && Text.StartsWith(" ", StringComparison.InvariantCultureIgnoreCase); }
        }

        #region Implementation of IEquatable<ResponseLine>

        public bool Equals(ResponseLine other) {
            return Text == other.Text;
        }

        #endregion
    }
}
