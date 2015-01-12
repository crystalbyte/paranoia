#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Awesomium.Core;

#endregion

namespace Crystalbyte.Paranoia.UI {
    internal static class CollectionExtensions {
        public static IEnumerable<JSValue> ToJsValues(this IEnumerable<object> source) {
            foreach (var item in source) {
                if (item is int) {
                    yield return new JSValue((int)item);
                    continue;
                }

                if (item is bool) {
                    yield return new JSValue((bool)item);
                    continue;
                }

                if (item is double) {
                    yield return new JSValue((double)item);
                    continue;
                }

                var s = item as string;
                if (s == null) 
                    throw new InvalidOperationException();

                yield return new JSValue(s);
            }
        }

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

        public static void AddRange<T, TS>(this Dictionary<T, TS> target, IDictionary<T, TS> source) {
            foreach (var item in source) {
                target.Add(item.Key, item.Value);
            }
        }
    }
}