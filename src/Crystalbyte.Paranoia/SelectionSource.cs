using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    public abstract class SelectionSource<T> where T : SelectionObject {
        private IEnumerable<T> _selection;

        public event EventHandler SelectionChanged;

        private void OnSelectionChanged() {
            var handler = SelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public IEnumerable<T> Selection {
            get { return _selection; }
            set {
                if (_selection == value) {
                    return;
                }

                _selection = value;
                OnSelectionChanged();
            }
        }
    }
}
