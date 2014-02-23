using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class SmtpAccountContext : NotificationObject {
        private readonly SmtpAccount _account;
        public SmtpAccountContext(SmtpAccount account) {
            _account = account;
        }

        public string Username {
            get { return _account.Username; }
            set {
                if (_account.Username == value) {
                    return;
                }

                RaisePropertyChanging(() => Username);
                _account.Username = value;
                RaisePropertyChanged(() => Username);
            }
        }

        public string Password {
            get { return _account.Password; }
            set {
                if (_account.Username == value) {
                    return;
                }

                RaisePropertyChanging(() => Password);
                _account.Password = value;
                RaisePropertyChanged(() => Password);

            }
        }

        public string Host {
            get { return _account.Host; }
            set {
                if (_account.Host == value) {
                    return;
                }

                RaisePropertyChanging(() => Host);
                _account.Username = value;
                RaisePropertyChanged(() => Host);
            }
        }

        public short Port {
            get { return _account.Port; }
            set {
                if (_account.Port == value) {
                    return;
                }

                RaisePropertyChanging(() => Port);
                _account.Port = value;
                RaisePropertyChanged(() => Port);
            }
        }

        public SecurityPolicy Security {
            get { return _account.Security; }
        }

    }
}
