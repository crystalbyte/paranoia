using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    public static class CollectionExtensions {
        public static void ForEach<T>(this ICollection<T> source, Action<T> action) {
            foreach (var item in source) {
                action(item);
            }
        }

        public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> source ) {
            foreach (var item in source) {
                target.Add(item);
            }
        }
    }
}
