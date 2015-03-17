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

namespace Crystalbyte.Paranoia.Mail.Mime.Traverse {
    /// <summary>
    ///     Finds the first <see cref="MessagePart" /> which have a given MediaType in a depth first traversal.
    /// </summary>
    internal class FindFirstMessagePartWithMediaType : IQuestionAnswerMessageTraverser<string, MessagePart> {
        /// <summary>
        ///     Finds the first <see cref="MessagePart" /> with the given MediaType
        /// </summary>
        /// <param name="message"> The <see cref="MailMessageReader" /> to start looking in </param>
        /// <param name="question"> The MediaType to look for. Case is ignored. </param>
        /// <returns>
        ///     A <see cref="MessagePart" /> with the given MediaType or <see langword="null" /> if no such
        ///     <see
        ///         cref="MessagePart" />
        ///     was found
        /// </returns>
        public MessagePart VisitMessage(MailMessageReader message, string question) {
            if (message == null)
                throw new ArgumentNullException("message");

            return VisitMessagePart(message.MessagePart, question);
        }

        /// <summary>
        ///     Finds the first <see cref="MessagePart" /> with the given MediaType
        /// </summary>
        /// <param name="messagePart"> The <see cref="MessagePart" /> to start looking in </param>
        /// <param name="question"> The MediaType to look for. Case is ignored. </param>
        /// <returns>
        ///     A <see cref="MessagePart" /> with the given MediaType or <see langword="null" /> if no such
        ///     <see
        ///         cref="MessagePart" />
        ///     was found
        /// </returns>
        public MessagePart VisitMessagePart(MessagePart messagePart, string question) {
            if (messagePart == null)
                throw new ArgumentNullException("messagePart");

            if (messagePart.ContentType.MediaType.Equals(question, StringComparison.OrdinalIgnoreCase))
                return messagePart;

            if (messagePart.IsMultiPart) {
                foreach (var part in messagePart.MessageParts) {
                    var result = VisitMessagePart(part, question);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }
    }
}