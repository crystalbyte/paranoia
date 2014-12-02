using System;
using System.Collections.Generic;

namespace Crystalbyte.Paranoia.UI {
    internal static class NavigationStore {
        private static readonly Stack<KeyValuePair<Type, object>> Arguments 
            = new Stack<KeyValuePair<Type, object>>();

        public static void Push(Type t, object argument) {
            Arguments.Push(new KeyValuePair<Type, object>(t, argument));
        }

        public static object Pop(Type t) {
            if (Arguments.Count == 0) {
                return null;
            }

            var pair = Arguments.Peek();
            return pair.Key == t ? Arguments.Pop().Value : null;
        }
    }
}
