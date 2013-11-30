using System.IO;
using System.Security.Cryptography;
using System.Text;
using Crystalbyte.Paranoia.Models;

namespace Crystalbyte.Paranoia.Contexts {

    public sealed class IdentityContext : NotificationObject {

        private readonly Identity _identity;
        private string _gravatarImageUrl;
        private bool _isSelected;

        public IdentityContext()
            : this(new Identity()) { }

        public IdentityContext(Identity identity) {
            _identity = identity;
        }

        public Identity Model {
            get { return _identity; }
        }

        private void CreateGravatarImageUrl() {
            using (var md5 = MD5.Create()) {
                var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(EmailAddress.Trim()));
                using (var writer = new StringWriter()) {
                    foreach (var b in bytes) {
                        writer.Write(b.ToString("x2"));
                    }
                    GravatarUrl = string.Format("http://www.gravatar.com/avatar/{0}?s=200&d=mm", writer);
                }
            }
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
                CreateGravatarImageUrl();
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
    }
}
