#region Using directives

using System;

#endregion

namespace Crystalbyte.Paranoia.UI {
    internal sealed class NavigationRequestedEventArgs : EventArgs {
        public NavigationRequestedEventArgs(Uri uri) {
            Target = uri;
        }

        public Uri Target { get; set; }
    }
}