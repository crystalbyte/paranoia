using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Crystalbyte.Paranoia.UI {
    public static class DeferredActions {

        private static readonly Stack<Action> Actions = new Stack<Action>();

        public static void Push(Action action) {
            Actions.Push(action);
        }

        public static Action Pop() {
            return Actions.Pop();
        }

        public static bool HasActions {
            get { return Actions.Count > 0; }
        }
    }
}
