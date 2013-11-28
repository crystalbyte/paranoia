using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Crystalbyte.Paranoia.Commands;
using System.Security.Cryptography;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Crystalbyte.Paranoia.Properties;

namespace Crystalbyte.Paranoia.Contexts {

    [Export, Shared]
    public sealed class IdentityScreenContext : NotificationObject, IDataErrorInfo {

        private bool _isActive;
        private string _name;
        private string _emailAddress;
        private string _notes;
        private string _gravatarUrl;

        public IdentityScreenContext() {
            CreateCommand = new RelayCommand(OnCanCreateCommandExecuted, OnCreateCommandExecuted);
            CancelCommand = new RelayCommand(OnCancelCommandExecuted);
        }

        public ICommand CreateCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        [Required(ErrorMessageResourceName = "NullOrEmptyErrorText")]
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

        private void OnEmailAddressChanged() {
            CreateGravatarUrl();
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

        private void OnCancelCommandExecuted(object obj) {
            IsActive = false;
        }

        private static bool OnCanCreateCommandExecuted(object parameter) {
            return true;
        }

        private void OnCreateCommandExecuted(object parameter) {
            throw new NotImplementedException();
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

        #region Implementation of IDataErrorInfo

        /// <summary>
        /// http://weblogs.asp.net/marianor/archive/2009/04/17/wpf-validation-with-attributes-and-idataerrorinfo-interface-in-mvvm.aspx
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public string this[string columnName] {
            get { return string.Empty; }
        }

        public string Error {
            get { return string.Empty; }
        }

        #endregion
    }
}
