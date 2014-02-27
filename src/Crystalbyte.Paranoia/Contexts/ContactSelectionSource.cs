using System;
using System.Composition;

namespace Crystalbyte.Paranoia.Contexts {
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

        public ContactContext Contact {
            get { return _selectedIdentity; }
            set {
                if (_selectedIdentity == value) {
                    return;
                }

                RaisePropertyChanging(() => Contact);
                _selectedIdentity = value;
                RaisePropertyChanged(() => Contact);

                OnSelectionChanged(EventArgs.Empty);
            }
        }
    }
}
