#region Using directives

using System;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class DocumentTextRequestedEventArgs : EventArgs {
        public string Document { get; set; }
    }
}