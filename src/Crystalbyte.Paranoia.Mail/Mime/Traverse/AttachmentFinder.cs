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
using System.Collections.Generic;

#endregion

namespace Crystalbyte.Paranoia.Mail.Mime.Traverse {
    /// <summary>
    ///     Finds all <see cref="MessagePart" />s which are considered to be attachments
    /// </summary>
    internal class AttachmentFinder : MultipleMessagePartFinder {
        protected override List<MessagePart> CaseLeaf(MessagePart messagePart) {
            if (messagePart == null)
                throw new ArgumentNullException("messagePart");

            // Maximum space needed is one
            var leafAnswer = new List<MessagePart>(1);

            if (messagePart.IsAttachment)
                leafAnswer.Add(messagePart);

            return leafAnswer;
        }
    }
}