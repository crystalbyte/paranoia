#region Using directives

using Crystalbyte.Paranoia.Data;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class MailContactContext : SelectionObject {
        private readonly MailContactModel _contact;
        private bool _isValidated;

        internal MailContactContext(MailContactModel contact) {
            _contact = contact;
        }

        public bool IsValidated {
            get { return _isValidated; }
            set {
                if (_isValidated == value) {
                    return;
                }
                _isValidated = value;
                RaisePropertyChanged(() => IsValidated);
            }
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