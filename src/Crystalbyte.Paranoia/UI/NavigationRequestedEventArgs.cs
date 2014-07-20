using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crystalbyte.Paranoia.Contexts {
    internal sealed class NavigationRequestedEventArgs : EventArgs {
        public NavigationRequestedEventArgs(Uri uri) {
            Target = uri;
        }

        public Uri Target { get; set; }
    }
}
