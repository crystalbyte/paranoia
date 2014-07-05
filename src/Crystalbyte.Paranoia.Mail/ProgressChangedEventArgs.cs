#region Using directives

using System;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    public sealed class ProgressChangedEventArgs : EventArgs {
        internal ProgressChangedEventArgs(Single percentage) {
            Percentage = percentage;
        }

        public Single Percentage { get; private set; }
    }
}