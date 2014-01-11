using Crystalbyte.Paranoia.Contexts;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    [Export, Shared]
    public sealed class ContactSelectionSource : NotificationObject {
        private ContactContext _selectedIdentity;

        #region Event Declarations

        public event EventHandler SelectionChanged;

        public void OnSelectionChanged(EventArgs e) {
            var handler = SelectionChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        public ContactContext Current {
            get { return _selectedIdentity; }
            set {
                if (_selectedIdentity == value) {
                    return;
                }

                RaisePropertyChanging(() => Current);
                _selectedIdentity = value;
                RaisePropertyChanged(() => Current);

                OnSelectionChanged(EventArgs.Empty);
            }
        }
    }
}
