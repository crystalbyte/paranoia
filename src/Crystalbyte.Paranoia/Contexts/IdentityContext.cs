#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Crystalbyte.Paranoia.Models;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class IdentityContext : NotificationObject {

        #region Private Fields

        private readonly Identity _identity;
        private readonly ObservableCollection<ContactContext> _contacts;
        private string _gravatarImageUrl;
        private bool _isSelected;

        #endregion

        #region Construction

        public IdentityContext()
            : this(new Identity()) { }

        public IdentityContext(Identity identity) {
            _identity = identity;
            _contacts = new ObservableCollection<ContactContext>();
        }

        #endregion

        public ObservableCollection<ContactContext> Contacts {
            get { return _contacts; }
        }

        public SmtpAccountContext SmtpAccount { get; set; }

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

        public string Address {
            get { return _identity.Address; }
            set {
                if (_identity.Address == value) {
                    return;
                }

                RaisePropertyChanging(() => Address);
                _identity.Address = value;
                RaisePropertyChanged(() => Address);
                CreateGravatarUrl();
            }
        }

        public void InvalidateContacts() {
            RaisePropertyChanged(() => Contacts);
        }

        private void CreateGravatarUrl() {
            _gravatarImageUrl = Gravatar.CreateImageUrl(Address);
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

            //LoadContacts();
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