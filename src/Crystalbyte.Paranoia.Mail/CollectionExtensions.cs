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
using System.Linq;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    internal static class CollectionExtensions {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            foreach (var item in source) {
                action(item);
            }
        }

        public static void AddRange<T>(this HashSet<T> target, IEnumerable<T> source) {
            foreach (var item in source) {
                target.Add(item);
            }
        }

        public static IEnumerable<IEnumerable<T>> Bundle<T>(this IEnumerable<T> source, int size) {
            // ReSharper disable PossibleMultipleEnumeration
            while (source.Any()) {
                yield return source.Take(size);
                source = source.Skip(size);
            }
            // ReSharper restore PossibleMultipleEnumeration
        }

        public static void AddRange<T, TS>(this IList<KeyValuePair<T, TS>> target, IList<KeyValuePair<T, TS>> source) {
            foreach (var item in source) {
                target.Add(new KeyValuePair<T, TS>(item.Key, item.Value));
            }
        }
    }
}