#region Using directives

using Crystalbyte.Paranoia.Data;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class MailContactContext : SelectionObject {
        private readonly MailContactModel _contact;

        internal MailContactContext(MailContactModel contact) {
            _contact = contact;
        }

        public string Address {
            get { return _contact.Address; }
            set {
                if (_contact.Address == value) {
                    return;
                }

                _contact.Address = value;
                RaisePropertyChanged(() => Address);
            }
        }

        public string Name {
            get { return _contact.Name; }
            set {
                if (_contact.Name == value) {
                    return;
                }

                _contact.Name = value;
                RaisePropertyChanged(() => Name);
            }
        }
    }
}