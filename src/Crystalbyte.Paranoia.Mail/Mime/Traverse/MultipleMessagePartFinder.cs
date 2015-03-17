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
    ///     An abstract class that implements the MergeLeafAnswers method.<br />
    ///     The method simply returns the union of all answers from the leaves.
    /// </summary>
    public abstract class MultipleMessagePartFinder : AnswerMessageTraverser<List<MessagePart>> {
        /// <summary>
        ///     Adds all the <paramref name="leafAnswers" /> in one big answer
        /// </summary>
        /// <param name="leafAnswers"> The answers to merge </param>
        /// <returns> A list with has all the elements in the <paramref name="leafAnswers" /> lists </returns>
        /// <exception cref="ArgumentNullException">
        ///     if
        ///     <paramref name="leafAnswers" />
        ///     is
        ///     <see langword="null" />
        /// </exception>
        protected override List<MessagePart> MergeLeafAnswers(List<List<MessagePart>> leafAnswers) {
            if (leafAnswers == null)
                throw new ArgumentNullException("leafAnswers");

            // We simply create a list with all the answer generated from the leaves
            var mergedResults = new List<MessagePart>();

            foreach (var leafAnswer in leafAnswers) {
                mergedResults.AddRange(leafAnswer);
            }

            return mergedResults;
        }
    }
}