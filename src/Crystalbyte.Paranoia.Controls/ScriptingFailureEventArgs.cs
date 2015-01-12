using System;

namespace Crystalbyte.Paranoia.UI {
    public sealed class ScriptingFailureEventArgs : EventArgs {
        public ScriptingFailureEventArgs(Exception ex) {
            Exception = ex;
        }

        public Exception Exception { get; private set; }
    }
}
