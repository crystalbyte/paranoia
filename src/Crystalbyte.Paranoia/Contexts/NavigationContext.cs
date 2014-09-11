using System;

namespace Crystalbyte.Paranoia {
    public sealed class NavigationContext : SelectionObject {
        public string Title { get; set; }
        public Uri TargetUri { get; set; }
    }
}
