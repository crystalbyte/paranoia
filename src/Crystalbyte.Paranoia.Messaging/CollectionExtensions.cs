#region Using directives

using System;
using System.Collections.Generic;

#endregion

namespace Crystalbyte.Paranoia.Messaging {
    internal static class CollectionExtensions {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            foreach (var item in source) {
                action(item);
            }
        }
    }
}