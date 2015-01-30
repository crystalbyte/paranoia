using System.Collections;
using System.Collections.Generic;

namespace Crystalbyte.Paranoia {
    public sealed class ItemSelectionRequestedEventArgs {
        internal ItemSelectionRequestedEventArgs(IEnumerable<object> pivotElements) {
            PivotElements = pivotElements;
        }

        public IEnumerable<object> PivotElements { get; private set; }
    }
}