using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    internal sealed class ProgressChangedEventArgs : EventArgs {
        public ProgressChangedEventArgs(Single percentage) {
            Percentage = percentage;
        }
        public Single Percentage { get; set; }
    }
}
