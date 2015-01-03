using System;
using System.Collections.Generic;

namespace Crystalbyte.Paranoia.UI {
    internal static class NavigationArguments {
        private static readonly Stack<object> Arguments
            = new Stack<object>();

        public static void Push(object argument) {
            Arguments.Push(argument);
        }

        public static object Pop() {
            if (Arguments.Count == 0) {
                throw new ArgumentNullException();
            }

            return Arguments.Pop();
        }
    }
}
