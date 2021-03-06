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
using System.Collections.Generic;

#endregion

namespace Crystalbyte.Paranoia.Mail.Mime.Traverse {
    /// <summary>
    ///     Finds all the <see cref="MessagePart" />s which have a given MediaType using a depth first traversal.
    /// </summary>
    internal class FindAllMessagePartsWithMediaType : IQuestionAnswerMessageTraverser<string, List<MessagePart>> {
        /// <summary>
        ///     Finds all the <see cref="MessagePart" />s with the given MediaType
        /// </summary>
        /// <param name="message"> The <see cref="MailMessageReader" /> to start looking in </param>
        /// <param name="question"> The MediaType to look for. Case is ignored. </param>
        /// <returns>
        ///     A List of <see cref="MessagePart" /> s with the given MediaType. <br /> <br /> The List might be empty if no such
        ///     <see
        ///         cref="MessagePart" />
        ///     s were found. <br /> The order of the elements in the list is the order which they are found using a depth first
        ///     traversal of the
        ///     <see
        ///         cref="MailMessageReader" />
        ///     hierarchy.
        /// </returns>
        public List<MessagePart> VisitMessage(MailMessageReader message, string question) {
            if (message == null)
                throw new ArgumentNullException("message");

            return VisitMessagePart(message.MessagePart, question);
        }

        /// <summary>
        ///     Finds all the <see cref="MessagePart" />s with the given MediaType
        /// </summary>
        /// <param name="messagePart"> The <see cref="MessagePart" /> to start looking in </param>
        /// <param name="question"> The MediaType to look for. Case is ignored. </param>
        /// <returns>
        ///     A List of <see cref="MessagePart" /> s with the given MediaType. <br /> <br /> The List might be empty if no such
        ///     <see
        ///         cref="MessagePart" />
        ///     s were found. <br /> The order of the elements in the list is the order which they are found using a depth first
        ///     traversal of the
        ///     <see
        ///         cref="MailMessageReader" />
        ///     hierarchy.
        /// </returns>
        public List<MessagePart> VisitMessagePart(MessagePart messagePart, string question) {
            if (messagePart == null)
                throw new ArgumentNullException("messagePart");

            var results = new List<MessagePart>();

            if (messagePart.ContentType.MediaType.Equals(question, StringComparison.OrdinalIgnoreCase))
                results.Add(messagePart);

            if (messagePart.IsMultiPart) {
                foreach (var part in messagePart.MessageParts) {
                    var result = VisitMessagePart(part, question);
                    results.AddRange(result);
                }
            }

            return results;
        }
    }
}