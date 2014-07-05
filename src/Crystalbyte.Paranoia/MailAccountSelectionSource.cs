using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    [Export]
    public sealed class MailAccountSelectionSource : NotificationObject {

        private MailAccountContext _selectedAccount;

        public event EventHandler SelectedAccountChanged;

        private void OnSelectedAccountChanged() {
            var handler = SelectedAccountChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public MailAccountContext SelectedAccount {
            get { return _selectedAccount; }
            set {
                if (_selectedAccount == value) {
                    return;
                }

                _selectedAccount = value;
                RaisePropertyChanged(() => SelectedAccount);
                OnSelectedAccountChanged();
            }
        }
    }
}
