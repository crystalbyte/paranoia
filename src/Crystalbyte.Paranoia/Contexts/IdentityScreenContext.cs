using System.Composition;
using System.IO;
using System.Text;
using System.Windows.Input;
using Crystalbyte.Paranoia.Commands;
using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.Data;

namespace Crystalbyte.Paranoia.Contexts {

    [Export, Shared]
    public sealed class IdentityScreenContext : ValidationObject<IdentityScreenContext> {

        #region Private Fields

        private bool _isActive;
        private string _name;
        private string _emailAddress;
        private string _notes;
        private string _gravatarUrl;

        private const string EmailRegexPattern = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";

        #endregion

        #region Construction

        public IdentityScreenContext() {
            CreateCommand = new RelayCommand(OnCanCreateCommandExecuted, OnCreateCommandExecuted);
            CancelCommand = new RelayCommand(OnCancelCommandExecuted);
        }

        #endregion

        #region Import Declarations

        [Import]
        public LocalStorage LocalStorage { get; set; }

        [Import]
        public AppContext AppContext { get; set; }

        #endregion

        public ICommand CreateCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NameRequiredErrorText")]
        [StringLength(64, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MaxStringLength64ErrorText")]
        public string Name {
            get { return _name; }
            set {
                if (_name == value) {
                    return;
                }

                RaisePropertyChanging(() => Name);
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        [RegularExpression(EmailRegexPattern, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "InvalidEmailFormatErrorText")]
        public string EmailAddress {
            get { return _emailAddress; }
            set {
                if (_emailAddress == value) {
                    return;
                }

                RaisePropertyChanging(() => EmailAddress);
                _emailAddress = value;
                RaisePropertyChanged(() => EmailAddress);
                OnEmailAddressChanged();
            }
        }

        public string GravatarUrl {
            get { return _gravatarUrl; }
            set {
                if (_gravatarUrl == value) {
                    return;
                }

                RaisePropertyChanging(() => GravatarUrl);
                _gravatarUrl = value;
                RaisePropertyChanged(() => GravatarUrl);
            }
        }

        public string Notes {
            get { return _notes; }
            set {
                if (_notes == value) {
                    return;
                }

                RaisePropertyChanging(() => Notes);
                _notes = value;
                RaisePropertyChanged(() => Notes);
            }
        }

        public bool IsActive {
            get { return _isActive; }
            set {
                if (_isActive == value) {
                    return;
                }

                RaisePropertyChanging(() => IsActive);
                _isActive = value;
                RaisePropertyChanged(() => IsActive);
            }
        }

        private void OnEmailAddressChanged() {
            CreateGravatarUrl();
        }

        private void OnCancelCommandExecuted(object obj) {
            Close();
        }

        private void Close() {
            ClearValues();
            IsActive = false;
        }

        private void ClearValues() {
            Name = string.Empty;
            EmailAddress = string.Empty;
            Notes = string.Empty;
            GravatarUrl = null;
        }

        private static bool OnCanCreateCommandExecuted(object parameter) {
            return true;
        }

        private async void OnCreateCommandExecuted(object parameter) {
            var identity = new IdentityContext {
                EmailAddress = EmailAddress,
                Notes = Notes,
                Name = Name,
                PrivateKey = "muh",
                PublicKey = "mäh"
            };

            await LocalStorage.InsertAsync(identity.Model);
            AppContext.Identities.Add(identity);

            Close();
        }

        public void CreateGravatarUrl() {
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
    }
}
