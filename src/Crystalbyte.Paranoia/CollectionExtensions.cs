#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Crystalbyte.Paranoia {
    public static class CollectionExtensions {
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int count) {
            return source
                .Select((x, i) => new {Index = i, Value = x})
                .GroupBy(x => x.Index%count)
                .Select(x => x.Select(y => y.Value).ToArray())
                .ToArray();
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            foreach (var item in source) {
                action(item);
            }
        }

        public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> source) {
            foreach (var item in source) {
                target.Add(item);
            }
        }

        public static void AddRange<T>(this IList<T> target, IEnumerable<T> source) {
            foreach (var item in source) {
                target.Add(item);
            }
        }
    }
}