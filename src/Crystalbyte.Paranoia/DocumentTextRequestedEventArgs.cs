using System;

namespace Crystalbyte.Paranoia {
    public sealed class DocumentTextRequestedEventArgs : EventArgs {
        public string Document { get; set; }
    }
}
