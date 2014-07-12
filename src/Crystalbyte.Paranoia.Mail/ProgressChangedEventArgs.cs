#region Using directives

using System;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    public sealed class ProgressChangedEventArgs : EventArgs {
        internal ProgressChangedEventArgs(long byteCount) {
            ByteCount = byteCount;
        }

        public long ByteCount { get; private set; }
    }
}