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