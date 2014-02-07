#region Using directives

using System.ComponentModel.DataAnnotations;
using System.Composition;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Input;
using Crystalbyte.Paranoia.Commands;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.Models;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    [Export, Shared]
    public sealed class CreateIdentityScreenContext : ValidationObject<CreateIdentityScreenContext> {

        #region Private Fields

        private bool _isActive;
        private string _name;
        private string _address;
        private string _notes;
        private string _gravatarUrl;

        #endregion

        #region Construction

        public CreateIdentityScreenContext() {
            CreateCommand = new RelayCommand(OnCanCreateCommandExecuted, OnCreateCommandExecuted);
            CancelCommand = new RelayCommand(OnCancelCommandExecuted);
        }

        #endregion

        #region Import Declarations

        [Import]
        public StorageContext LocalStorage { get; set; }

        [Import]
        public AppContext AppContext { get; set; }

        #endregion

        public ICommand CreateCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NameRequiredErrorText")]
        [StringLength(64, ErrorMessageResourceType = typeof(Resources),
            ErrorMessageResourceName = "MaxStringLength64ErrorText")]
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
        [RegularExpression(RegexPatterns.Email, ErrorMessageResourceType = typeof(Resources),
            ErrorMessageResourceName = "InvalidEmailFormatErrorText")]
        public string Address {
            get { return _address; }
            set {
                if (_address == value) {
                    return;
                }

                RaisePropertyChanging(() => Address);
                _address = value;
                RaisePropertyChanged(() => Address);
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
            Address = string.Empty;
            Notes = string.Empty;
            GravatarUrl = null;
        }

        private static bool OnCanCreateCommandExecuted(object parameter) {
            return true;
        }

        private void OnCreateCommandExecuted(object parameter) {
            var identity = new Identity {
                Address = Address,
                Notes = Notes,
                Name = Name
            };

            AppContext.Identities.Add(new IdentityContext(identity));

            //LocalStorage.Context.Identities.Add(identity);
            //LocalStorage.Context.SaveChanges();

            Close();
        }

        public void CreateGravatarUrl() {
            using (var md5 = MD5.Create()) {
                var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(Address.Trim()));
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