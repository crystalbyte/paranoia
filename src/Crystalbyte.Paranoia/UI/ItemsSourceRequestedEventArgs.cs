#region Using directives

using System;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public sealed class ItemsSourceRequestedEventArgs : EventArgs {
        public ItemsSourceRequestedEventArgs(string text) {
            Text = text;
        }

        public string Text { get; private set; }
        public object ItemsSource { get; set; }
    }
}