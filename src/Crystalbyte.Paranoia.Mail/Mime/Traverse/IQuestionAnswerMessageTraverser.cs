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

namespace Crystalbyte.Paranoia.Mail.Mime.Traverse {
    /// <summary>
    ///     This interface describes a MessageTraverser which is able to traverse a Message structure
    ///     and deliver some answer given some question.
    /// </summary>
    /// <typeparam name="TAnswer"> This is the type of the answer you want to have delivered. </typeparam>
    /// <typeparam name="TQuestion"> This is the type of the question you want to have answered. </typeparam>
    public interface IQuestionAnswerMessageTraverser<TQuestion, TAnswer> {
        /// <summary>
        ///     Call this when you want to apply this traverser on a <see cref="MailMessageReader" />.
        /// </summary>
        /// <param name="message">
        ///     The <see cref="MailMessageReader" /> which you want to traverse. Must not be
        ///     <see langword="null" /> .
        /// </param>
        /// <param name="question"> The question </param>
        /// <returns> An answer </returns>
        TAnswer VisitMessage(MailMessageReader message, TQuestion question);

        /// <summary>
        ///     Call this when you want to apply this traverser on a <see cref="MessagePart" />.
        /// </summary>
        /// <param name="messagePart">
        ///     The <see cref="MessagePart" /> which you want to traverse. Must not be
        ///     <see langword="null" /> .
        /// </param>
        /// <param name="question"> The question </param>
        /// <returns> An answer </returns>
        TAnswer VisitMessagePart(MessagePart messagePart, TQuestion question);
    }
}