using Crystalbyte.Paranoia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class ContactContext : NotificationObject {
        private readonly Contact _contact;

        public ContactContext()
            : this(new Contact()) { }

        public ContactContext(Contact contact) {
            _contact = contact;
        }

        public string Name {
            get { return _contact.Name; }
            set {
                if (_contact.Name == value) {
                    return;
                }

                RaisePropertyChanging(() => Name);
                _contact.Name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public string EmailAddress {
            get { return _contact.EmailAddress; }
            set {
                if (_contact.EmailAddress == value) {
                    return;
                }

                RaisePropertyChanging(() => EmailAddress);
                _contact.EmailAddress = value;
                RaisePropertyChanged(() => EmailAddress);
            }
        }

        public ContactRequestStatus RequestStatus {
            get { return (ContactRequestStatus)_contact.RequestStatus; }
            set {
                if (_contact.RequestStatus == (byte) value) {
                    return;
                }

                RaisePropertyChanging(() => RequestStatus);
                _contact.RequestStatus = (byte)value;
                RaisePropertyChanged(() => RequestStatus);
            }
        }
    }
}
