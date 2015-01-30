using System.Collections.Generic;

namespace Crystalbyte.Paranoia.UI {
    public sealed class ItemSelectionRequestedEventArgs {
        internal ItemSelectionRequestedEventArgs(SelectionPosition position, IEnumerable<object> pivotElements = null) {
            PivotElements = pivotElements;
            Position = position;
        }
        
        public IEnumerable<object> PivotElements { get; private set; }
        public SelectionPosition Position { get; private set; }
    }
}