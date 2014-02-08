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
using System;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class IdentityCreationContext : ValidationObject<IdentityCreationContext> {

        #region Private Fields

        private bool _isActive;
        private string _name;
        private string _address;
        private string _notes;
        private string _gravatarUrl;

        #endregion

        #region Construction

        public IdentityCreationContext() {
            ContinueCommand = new RelayCommand(OnCanContinueCommandExecuted, OnContinueCommandExecuted);
            CancelCommand = new RelayCommand(OnCancelCommandExecuted);
        }

        #endregion

        public event EventHandler Finished;
        private void OnFinished() {
            var handler = Finished;
            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
        }

        public ICommand ContinueCommand { get; set; }
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
            OnFinished();
        }

        private static bool OnCanContinueCommandExecuted(object parameter) {
            return true;
        }

        private void OnContinueCommandExecuted(object parameter) {
            var identity = new Identity {
                Address = Address,
                Name = Name
            };

            
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