using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Crystalbyte.Paranoia.Commands;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.Models;

namespace Crystalbyte.Paranoia.Contexts {
    [Export, Shared]
    public sealed class AddContactScreenContext : ValidationObject<AddContactScreenContext> {

        #region Private Fields

        private bool _isActive;
        private string _emailAddress;
        private string _gravatarImageUrl;
        private string _name;

        #endregion

        #region Construction

        public AddContactScreenContext() {
            CreateCommand = new RelayCommand(OnCreateCommandExecuted);
            CancelCommand = new RelayCommand(OnCancelCommandExecuted);
        }

        private void OnCreateCommandExecuted(object obj) {
            var contact = new Contact {
                EmailAddress = EmailAddress,
                RequestStatus = (byte)ContactRequestStatus.Pending,
                PublicKey = "public-key",
                Name = Name
            };

            var identity = IdentitySelectionSource.Current;
            identity.AddContact(contact);

            LocalStorage.Context.SaveChanges();
            Close();
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
            GravatarUrl = null;
        }

        #endregion

        #region Import Declarations

        [Import]
        public AppContext AppContext { get; set; }

        [Import]
        public LocalStorage LocalStorage { get; set; }

        [Import]
        public IdentitySelectionSource IdentitySelectionSource { get; set; }

        #endregion

        public ICommand CreateCommand { get; set; }
        public ICommand CancelCommand { get; set; }

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
        public string EmailAddress {
            get { return _emailAddress; }
            set {
                if (_emailAddress == value) {
                    return;

                }

                RaisePropertyChanging(() => EmailAddress);
                _emailAddress = value;
                RaisePropertyChanged(() => EmailAddress);
                CreateGravatarImageUrl();
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
            GravatarUrl = Gravatar.CreateImageUrl(EmailAddress);
        }
    }
}
