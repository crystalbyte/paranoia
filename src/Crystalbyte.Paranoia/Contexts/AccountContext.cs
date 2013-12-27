#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Commands;
using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Models;
using System.Xml.Serialization;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class AccountContext : ValidationObject<AccountContext> {

        #region Private Fields

        private readonly ImapAccount _imap;
        private readonly SmtpAccount _smtp;
        private bool _isSelected;

        #endregion

        #region Construction

        public AccountContext(ImapAccount imap, SmtpAccount smtp) {
            _imap = imap;
            _smtp = smtp;
        }

        #endregion

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

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        public SecurityPolicy ImapSecurity {
            get { return (SecurityPolicy)_imap.Security; }
            set {
                if (_imap.Security == (short)value) {
                    return;
                }

                RaisePropertyChanging(() => ImapSecurity);
                _imap.Security = (short)value;
                RaisePropertyChanged(() => ImapSecurity);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        public string ImapHost {
            get { return _imap.Host; }
            set {
                if (_imap.Host == value) {
                    return;
                }

                RaisePropertyChanging(() => ImapHost);
                _imap.Host = value;
                RaisePropertyChanged(() => ImapHost);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        public short ImapPort {
            get { return _imap.Port; }
            set {
                if (_imap.Port == value) {
                    return;
                }

                RaisePropertyChanging(() => ImapPort);
                if (value < 0) {
                    value = 0;
                }
                if (value > short.MaxValue) {
                    value = short.MaxValue;
                }
                _imap.Port = value;
                RaisePropertyChanged(() => ImapPort);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        public string ImapUsername {
            get { return _imap.Username; }
            set {
                if (_imap.Username == value) {
                    return;
                }

                RaisePropertyChanging(() => ImapUsername);
                _imap.Username = value;
                RaisePropertyChanged(() => ImapUsername);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "PasswordRequiredErrorText")]
        public string ImapPassword {
            get { return _imap.Password; }
            set {
                if (_imap.Password == value) {
                    return;
                }

                RaisePropertyChanging(() => ImapPassword);
                _imap.Password = value;
                RaisePropertyChanged(() => ImapPassword);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        public string SmtpPassword {
            get { return _smtp.Password; }
            set {
                if (_smtp.Password == value) {
                    return;
                }

                RaisePropertyChanging(() => SmtpPassword);
                _smtp.Password = value;
                RaisePropertyChanged(() => SmtpPassword);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        public string SmtpUsername {
            get { return _smtp.Username; }
            set {
                if (_smtp.Username == value) {
                    return;
                }

                RaisePropertyChanging(() => SmtpUsername);
                _smtp.Username = value;
                RaisePropertyChanged(() => SmtpUsername);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        public string SmtpHost {
            get { return _smtp.Host; }
            set {
                if (_smtp.Host == value) {
                    return;
                }

                RaisePropertyChanging(() => SmtpHost);
                _smtp.Host = value;
                RaisePropertyChanged(() => SmtpHost);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        public short SmtpPort {
            get { return _smtp.Port; }
            set {
                if (_smtp.Port == value) {
                    return;
                }

                RaisePropertyChanging(() => SmtpPort);
                if (value < 0) {
                    value = 0;
                }
                if (value > short.MaxValue) {
                    value = short.MaxValue;
                }
                _smtp.Port = value;
                RaisePropertyChanged(() => SmtpPort);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        public SecurityPolicy SmtpSecurity {
            get { return (SecurityPolicy)_smtp.Security; }
            set {
                if (_smtp.Security == (short)value) {
                    return;
                }

                RaisePropertyChanging(() => SmtpSecurity);
                _smtp.Security = (short)value;
                RaisePropertyChanged(() => SmtpSecurity);
            }
        }
    }
}