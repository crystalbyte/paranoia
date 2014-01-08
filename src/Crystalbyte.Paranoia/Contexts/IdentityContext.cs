#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Crystalbyte.Paranoia.Models;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class IdentityContext : NotificationObject {

        #region Private Fields

        private readonly Identity _identity;
        private string _gravatarImageUrl;
        private bool _isSelected;
        private readonly ObservableCollection<ContactContext> _contacts;

        #endregion

        #region Construction

        public IdentityContext()
            : this(new Identity()) { }

        public IdentityContext(Identity identity) {
            _identity = identity;
            _contacts = new ObservableCollection<ContactContext>();
        }

        #endregion

        public void AddContact(Contact contact) {
            _identity.Contacts.Add(contact);                
            _contacts.Add(new ContactContext(contact));
        }

        public ObservableCollection<ContactContext> Contacts {
            get { return _contacts; }
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
                CreateGravatarUrl();
            }
        }

        public void InvalidateContacts() {
            RaisePropertyChanged(() => Contacts);
        }

        private void CreateGravatarUrl() {
            _gravatarImageUrl = Gravatar.CreateImageUrl(EmailAddress);
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

        public string PublicKey {
            get { return _identity.PublicKey; }
            set {
                if (_identity.PublicKey == value) {
                    return;
                }

                RaisePropertyChanging(() => PublicKey);
                _identity.PublicKey = value;
                RaisePropertyChanged(() => PublicKey);
            }
        }

        public string PrivateKey {
            get { return _identity.PrivateKey; }
            set {
                if (_identity.PrivateKey == value) {
                    return;
                }

                RaisePropertyChanging(() => PrivateKey);
                _identity.PrivateKey = value;
                RaisePropertyChanged(() => PrivateKey);
            }
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
                OnSelected(EventArgs.Empty);
            }
        }

        public event EventHandler Selected;

        private void OnSelected(EventArgs e) {
            var handler = Selected;
            if (handler != null) {
                handler(this, e);
            }

            LoadContacts();
        }

        private void LoadContacts() {
            _contacts.Clear();
            _contacts.AddRange(_identity.Contacts.Select(x => new ContactContext(x)));
        }

        public string GravatarUrl {
            get {
                if (string.IsNullOrWhiteSpace(_gravatarImageUrl)) {
                    CreateGravatarUrl();
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
    }
}