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
using System.Security;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class IdentityCreationContext : ValidationObject<IdentityCreationContext> {

        #region Private Fields

        private bool _isActive;
        private string _name;
        private string _address;
        private string _gravatarUrl;
        private static string _password;
        private string _passwordConfirmation;

        #endregion

        #region Construction

        public IdentityCreationContext() {
            ContinueCommand = new RelayCommand(OnCanContinueCommandExecuted, OnContinueCommandExecuted);
            CancelCommand = new RelayCommand(OnCancelCommandExecuted);
        }

        #endregion

        private void ClearPassword() {
            _password = string.Empty;
            _passwordConfirmation = string.Empty;
        }

        public static string GetPassword() { 
            return _password; 
        }

        public event EventHandler Finished;

        private void OnFinished() {
            var handler = Finished;
            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
        }

        #region Overrides for ValidationObject<T>

        protected override void OnValidated(EventArgs e) {
            base.OnValidated(e);
            ContinueCommand.Refresh();
        }

        #endregion

        public RelayCommand ContinueCommand { get; set; }
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
                OnAddressChanged();
            }
        }

        [PasswordPolicy(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "PasswordComplexityInsufficientErrorText")]
        public string Password {
            get { return _password; }
            set {
                if (_password == value) {
                    return;
                }

                RaisePropertyChanging(() => Password);
                _password = value;
                RaisePropertyChanged(() => Password);
            }
        }

        [PasswordMatch(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "PasswordNotMatchingErrorText")]
        public string PasswordConfirmation {
            get { return _passwordConfirmation; }
            set {
                if (_passwordConfirmation == value) {
                    return;
                }

                RaisePropertyChanging(() => PasswordConfirmation);
                _passwordConfirmation = value;
                RaisePropertyChanged(() => PasswordConfirmation);
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

        private void OnAddressChanged() {
            CreateGravatarUrl();
        }

        private void OnCancelCommandExecuted(object obj) {
            ClearPassword();
            OnFinished();
        }

        private bool OnCanContinueCommandExecuted(object parameter) {
            return ValidFor(() => Address)
                && ValidFor(() => Name)
                && ValidFor(() => Password)
                && ValidFor(() => PasswordConfirmation);
        }

        private void OnContinueCommandExecuted(object parameter) {
            
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

        private sealed class PasswordMatchAttribute : ValidationAttribute {
            public override bool IsValid(object value) {
                var password = value as string;
                if (string.IsNullOrWhiteSpace(password)) {
                    return false;
                }
                return password == IdentityCreationContext.GetPassword();
            }
        }
    }
}