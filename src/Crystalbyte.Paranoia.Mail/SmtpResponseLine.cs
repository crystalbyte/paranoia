#region Using directives

using System;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    /// <summary>
    ///     Represents a single SMTP protocol response line.
    ///     http://www.greenend.org.uk/rjk/tech/smtpreplies.html
    /// </summary>
    internal struct SmtpResponseLine : IEquatable<SmtpResponseLine> {
        #region Construction

        public SmtpResponseLine(string text)
            : this() {
            Text = text;
        }

        #endregion

        public string Text { get; private set; }

        public int ResponseCode {
            get {
                int result;
                var success = int.TryParse(Text.Substring(0, 3), out result);
                return success ? result : -1;
            }
        }

        public bool IsTerminated {
            get {
                return !string.IsNullOrWhiteSpace(Text)
                       && Text.Length >= 4 && Text[3] == ' ';
            }
        }

        #region Implementation of IEquatable<SmtpResponseLine>

        public bool Equals(SmtpResponseLine other) {
            return Text == other.Text;
        }

        #endregion

        public bool IsServiceReady {
            get {
                if (string.IsNullOrWhiteSpace(Text) || Text.Length < 3) {
                    return false;
                }

                return Text.StartsWith("220");
            }
        }

        public bool IsOk {
            get {
                if (string.IsNullOrWhiteSpace(Text) || Text.Length < 3) {
                    return false;
                }
                return Text.StartsWith("250");
            }
        }

        public string Content {
            get {
                if (string.IsNullOrWhiteSpace(Text) || Text.Length <= 4) {
                    return string.Empty;
                }
                return Text.Substring(4, Text.Length - 4);
            }
        }

        public bool IsError {
            get {
                if (string.IsNullOrWhiteSpace(Text) || Text.Length < 3) {
                    return false;
                }
                return Text.StartsWith("5");
            }
        }

        public bool IsAuthenticated {
            get {
                if (string.IsNullOrWhiteSpace(Text) || Text.Length < 3) {
                    return false;
                }
                return Text.StartsWith("235");
            }
        }

        public bool IsContinuationRequest {
            get {
                if (string.IsNullOrWhiteSpace(Text) || Text.Length < 3) {
                    return false;
                }
                return Text.StartsWith("354");
            }
        }

        public bool IsPasswordRequest {
            get {
                if (string.IsNullOrWhiteSpace(Text) || Text.Length < 3) {
                    return false;
                }
                return Text.StartsWith("334");
            }
        }
    }
}