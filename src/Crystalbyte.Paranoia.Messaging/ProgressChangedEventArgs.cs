using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Messaging {
    public sealed class ProgressChangedEventArgs : EventArgs {
        internal ProgressChangedEventArgs(Single percentage) {
            Percentage = percentage;
        }

        public Single  Percentage { get; private set; }
    }
}
