using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.UI {
    public sealed class ItemsSourceRequestedEventArgs : EventArgs {
        public ItemsSourceRequestedEventArgs(string text) {
            Text = text;
        }

        public string Text { get; private set; }
    }
}
