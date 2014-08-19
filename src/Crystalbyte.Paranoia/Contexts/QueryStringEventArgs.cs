#region Using directives

using System;

#endregion

namespace Crystalbyte.Paranoia {
    internal sealed class QueryStringEventArgs : EventArgs {
        public QueryStringEventArgs(string text) {
            Text = text;
        }

        public string Text { get; private set; }
    }
}