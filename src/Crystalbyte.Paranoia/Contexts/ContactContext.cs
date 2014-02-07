using System.Collections.ObjectModel;
using Crystalbyte.Paranoia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class ContactContext : NotificationObject {

        #region Private Fields

        private readonly Contact _contact;
        private string _gravatarImageUrl;
        private bool _isSelected;
        private readonly ObservableCollection<MessageContext> _messages;

        #endregion

        #region Construction

        public ContactContext()
            : this(new Contact()) {
        }

        public ContactContext(Contact contact) {
            _contact = contact;
            _messages = new ObservableCollection<MessageContext>();
        }

        #endregion

        public Contact Model {
            get { return _contact; }
        }

        public ObservableCollection<MessageContext> Messages {
            get { return _messages; }
        }

        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) {
                    return;
                }

                RaisePropertyChanging(() => IsSelected);
                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
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

        public string Address {
            get { return _contact.Address; }
            set {
                if (_contact.Address == value) {
                    return;
                }

                RaisePropertyChanging(() => Address);
                _contact.Address = value;
                RaisePropertyChanged(() => Address);
            }
        }

        public ContactRequest ContactRequest {
            get { return (ContactRequest)_contact.ContactRequest; }
            set {
                if (_contact.ContactRequest == value) {
                    return;
                }

                RaisePropertyChanging(() => ContactRequest);
                _contact.ContactRequest = value;
                RaisePropertyChanged(() => ContactRequest);
            }
        }

        public string GravatarUrl {
            get {
                if (string.IsNullOrWhiteSpace(_gravatarImageUrl)) {
                    CreateGravatarImageUrl();
                }
                return _gravatarImageUrl;
            }
            set {
                if (_gravatarImageUrl == value) {
                    return;
                }

                RaisePropertyChanging(() => GravatarUrl);
                _gravatarImageUrl = value;
                RaisePropertyChanged(() => GravatarUrl);
            }
        }

        private void CreateGravatarImageUrl() {
            GravatarUrl = Gravatar.CreateImageUrl(Address);
        }
    }
}
