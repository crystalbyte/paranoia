#region Using directives

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
            while (source.Any()) {
                yield return source.Take(size);
                source = source.Skip(size);
            }
        }

        public static void AddRange<T, S>(this IList<KeyValuePair<T, S>> target, IList<KeyValuePair<T, S>> source) {
            foreach (var item in source) {
                target.Add(new KeyValuePair<T, S>(item.Key, item.Value));
            }
        }
    }
}