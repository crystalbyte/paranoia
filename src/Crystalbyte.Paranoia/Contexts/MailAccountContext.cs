using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;

namespace Crystalbyte.Paranoia {

    public sealed class MailAccountContext : SelectionObject {
        private readonly MailAccount _account;
        private readonly ObservableCollection<MailContactContext> _contacts;

        public MailAccountContext(MailAccount account) {
            _account = account;
            _contacts = new ObservableCollection<MailContactContext>();
        }

        protected async override void OnSelectionChanged() {
            base.OnSelectionChanged();

            ClearContacts();
            if (!IsSelected)
                return;

            await LoadContactsAsync();
        }

        internal void ClearContacts() {
            _contacts.Clear();
        }

        internal async Task LoadContactsAsync() {
            var contacts = await Task.Factory.StartNew(() => {
                using (var context = new DatabaseContext()) {
                    context.MailAccounts.Attach(_account);
                    return _account.Contacts.ToArray();
                }
            });
            _contacts.AddRange(contacts.Select(x => new MailContactContext(x)));
        }

        public string Address {
            get { return _account.Address; }
            set {
                if (_account.Address == value) {
                    return;
                }

                _account.Address = value;
                RaisePropertyChanged(() => Address);
            }
        }

        public string Name {
            get { return _account.Name; }
            set {
                if (_account.Name == value) {
                    return;
                }

                _account.Name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public IEnumerable<MailContactContext> Contacts {
            get { return _contacts; }
        }
    }
}
