using System;
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
            return !string.IsNullOrWhiteSpace(Text) && Text.StartsWith(id);
        }

        public bool IsBad {
            get { return !string.IsNullOrWhiteSpace(Text) && Text.Contains(" BAD "); }
        }

        public bool IsNo {
            get { return !string.IsNullOrWhiteSpace(Text) && Text.Contains(" NO "); }
        }

        public bool IsOk {
            get { return !string.IsNullOrWhiteSpace(Text) && Text.Contains(" OK "); }
        }

        public bool IsUntagged {
            get { return !string.IsNullOrWhiteSpace(Text) && Text.StartsWith("*"); }
        }

        public bool IsUntaggedOk {
            get { return !string.IsNullOrWhiteSpace(Text) && Text.StartsWith("* OK"); }
        }

        public bool IsContinuation {
            get { return !string.IsNullOrWhiteSpace(Text) && Text.StartsWith("+"); }
        }

        public bool IsMultiline {
            get { return !string.IsNullOrWhiteSpace(Text) && Text.StartsWith(" "); }
        }

        #region Implementation of IEquatable<ResponseLine>

        public bool Equals(ResponseLine other) {
            return Text == other.Text;
        }

        #endregion
    }
}
