#region Using directives

using System.ComponentModel.DataAnnotations;
using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Models;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class ImapAccountContext : NotificationObject {

        #region Private Fields

        private readonly ImapAccount _imap;
        private bool _isSelected;

        #endregion

        #region Construction

        public ImapAccountContext(ImapAccount imap) {
            _imap = imap;
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

        public SecurityPolicy ImapSecurity {
            get { return (SecurityPolicy) _imap.Security; }
            set {
                if (_imap.Security == (short) value) {
                    return;
                }

                RaisePropertyChanging(() => ImapSecurity);
                _imap.Security = (short) value;
                RaisePropertyChanged(() => ImapSecurity);
            }
        }

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

        public string Name {
            get { return _imap.Name; }
            set {
                if (_imap.Name == value) {
                    return;
                }

                RaisePropertyChanging(() => Name);
                _imap.Name = value;
                RaisePropertyChanged(() => Name);
            }
        }

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

        public string SmtpPassword {
            get { return _imap.SmtpAccount.Password; }
            set {
                if (_imap.SmtpAccount.Password == value) {
                    return;
                }

                RaisePropertyChanging(() => SmtpPassword);
                _imap.SmtpAccount.Password = value;
                RaisePropertyChanged(() => SmtpPassword);
            }
        }

        public string SmtpUsername {
            get { return _imap.SmtpAccount.Username; }
            set {
                if (_imap.SmtpAccount.Username == value) {
                    return;
                }

                RaisePropertyChanging(() => SmtpUsername);
                _imap.SmtpAccount.Username = value;
                RaisePropertyChanged(() => SmtpUsername);
            }
        }

        public string SmtpHost {
            get { return _imap.SmtpAccount.Host; }
            set {
                if (_imap.SmtpAccount.Host == value) {
                    return;
                }

                RaisePropertyChanging(() => SmtpHost);
                _imap.SmtpAccount.Host = value;
                RaisePropertyChanged(() => SmtpHost);
            }
        }

        public short SmtpPort {
            get { return _imap.SmtpAccount.Port; }
            set {
                if (_imap.SmtpAccount.Port == value) {
                    return;
                }

                RaisePropertyChanging(() => SmtpPort);
                if (value < 0) {
                    value = 0;
                }
                if (value > short.MaxValue) {
                    value = short.MaxValue;
                }
                _imap.SmtpAccount.Port = value;
                RaisePropertyChanged(() => SmtpPort);
            }
        }

        public SecurityPolicy SmtpSecurity {
            get { return (SecurityPolicy)_imap.SmtpAccount.Security; }
            set {
                if (_imap.SmtpAccount.Security == (short)value) {
                    return;
                }

                RaisePropertyChanging(() => SmtpSecurity);
                _imap.SmtpAccount.Security = (short)value;
                RaisePropertyChanged(() => SmtpSecurity);
            }
        }
    }
}