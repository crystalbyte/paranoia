using Crystalbyte.Paranoia.Contexts;
using System;
using System.Composition;

namespace Crystalbyte.Paranoia {

    [Export, Shared]
    public sealed class IdentitySelectionSource : NotificationObject {
        private IdentityContext _selectedIdentity;

        #region Event Declarations

        public event EventHandler SelectionChanged;

        public void OnSelectionChanged(EventArgs e) {
            var handler = SelectionChanged;
            if (handler != null) 
                handler(this, e);
        }

        #endregion

        public IdentityContext Identity {
            get { return _selectedIdentity; }
            set {
                if (_selectedIdentity == value) {
                    return;
                }

                RaisePropertyChanging(() => Identity);
                _selectedIdentity = value;
                RaisePropertyChanged(() => Identity);
                OnSelectionChanged(EventArgs.Empty);
            }
        }
    }
}
