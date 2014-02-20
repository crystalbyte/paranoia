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
    public sealed class ContactInvitationContext : ValidationObject<ContactInvitationContext> {

        #region Private Fields

        private bool _isActive;
        private string _emailAddress;
        private string _gravatarImageUrl;

        #endregion

        #region Construction

        public ContactInvitationContext() {
            CreateCommand = new RelayCommand(OnCreate);
            CancelCommand = new RelayCommand(OnCancel);
        }

        #endregion

        #region Import Declarations

        [Import]
        public AppContext AppContext { get; set; }

        [Import]
        public IdentitySelectionSource IdentitySelectionSource { get; set; }

        #endregion

        #region Event Declarations

        public event EventHandler Finished;
        private void OnFinished() {
            var handler = Finished;
            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion

        private async void OnCreate(object obj) {
            var contact = new Contact {
                Address = Address,
                Name = string.Empty,
                ContactRequest = ContactRequest.Pending
            };

            var identity = IdentitySelectionSource.Identity;

            var context = await identity.AddContactAsync(contact);
            context.IsSelected = true;
            identity.Contacts.Add(context);
            Close();

            await context.SendInviteAsync();
        }

        private void OnCancel(object obj) {
            Close();
        }

        private void Close() {
            ClearValues();
            IsActive = false;
            OnFinished();
        }

        private void ClearValues() {
            Address = string.Empty;
            GravatarUrl = null;
        }
        
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
                OnIsActiveChanged();
            }
        }

        private void OnIsActiveChanged() {
            // Nada ...
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        [RegularExpression(RegexPatterns.Email, ErrorMessageResourceType = typeof(Resources),
            ErrorMessageResourceName = "InvalidEmailFormatErrorText")]
        public string Address {
            get { return _emailAddress; }
            set {
                if (_emailAddress == value) {
                    return;

                }

                RaisePropertyChanging(() => Address);
                _emailAddress = value;
                RaisePropertyChanged(() => Address);
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
            GravatarUrl = Gravatar.CreateImageUrl(Address);
        }
    }
}
