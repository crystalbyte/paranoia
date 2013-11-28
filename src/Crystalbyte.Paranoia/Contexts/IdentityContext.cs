using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Models;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class IdentityContext : NotificationObject {
        private readonly Identity _identity;

        public IdentityContext()
            : this(new Identity()) { }

        public IdentityContext(Identity identity) {
            _identity = identity;
        }

        public Identity Model {
            get { return _identity; }
        }

        public string Name {
            get { return _identity.Name; }
            set {
                if (_identity.Name == value) {
                    return;
                }

                RaisePropertyChanging(() => Name);
                _identity.Name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public string EmailAddress {
            get { return _identity.EmailAddress; }
            set {
                if (_identity.EmailAddress == value) {
                    return;
                }

                RaisePropertyChanging(() => EmailAddress);
                _identity.EmailAddress = value;
                RaisePropertyChanged(() => EmailAddress);
            }
        }

        public string Notes {
            get { return _identity.Notes; }
            set {
                if (_identity.Notes == value) {
                    return;
                }

                RaisePropertyChanging(() => Notes);
                _identity.Notes = value;
                RaisePropertyChanged(() => Notes);
            }
        }
    }
}
