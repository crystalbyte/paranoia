using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Contexts;

namespace Crystalbyte.Paranoia {
    [Export, Shared]
    public sealed class ImapAccountSelectionSource : NotificationObject {
        private ImapAccountContext _selectedAccount;

        #region Event Declarations

        public event EventHandler SelectionChanged;

        public void OnSelectionChanged(EventArgs e) {
            var handler = SelectionChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        public ImapAccountContext Current {
            get { return _selectedAccount; }
            set {
                if (_selectedAccount == value) {
                    return;
                }

                RaisePropertyChanging(() => Current);
                _selectedAccount = value;
                RaisePropertyChanged(() => Current);

                OnSelectionChanged(EventArgs.Empty);
            }
        }
    }
}
