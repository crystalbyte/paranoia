#region Using directives

using System;
using System.Collections.Generic;

#endregion

namespace Crystalbyte.Paranoia {
    public static class CollectionExtensions {
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
    }
}