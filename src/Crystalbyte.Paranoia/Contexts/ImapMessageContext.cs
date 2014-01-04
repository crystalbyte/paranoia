#region Using directives

using System;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Messaging;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class ImapMessageContext : NotificationObject, IHtmlSource {
        private readonly ImapEnvelope _envelope;
        private readonly AccountContext _account;
        private readonly string _mailbox;
        private string _markup;
        private bool _isBusy;

        public ImapMessageContext(AccountContext account, ImapEnvelope envelope, string mailbox) {
            _envelope = envelope;
            _account = account;
            _mailbox = mailbox;
        }

        public DateTime? Date {
            get { return _envelope.InternalDate; }
        }

        public string Subject {
            get { return _envelope.Subject; }
        }

        /// <summary>
        ///   This key identifies the message across servers, users and mailboxes.
        /// </summary>
        public string Key {
            get { return string.Format("{0}@{1}:{2}", _account.ImapUsername, _account.ImapHost, _mailbox); }
        }

        public AccountContext Account {
            get { return _account; }
        }

        public string Mailbox {
            get { return _mailbox; }
        }

        public long Uid {
            get { return _envelope.Uid; }
        }

        public bool IsSeen {
            get { return _envelope.Flags.Any(x => x.ContainsIgnoreCase("\\Seen")); }
        }

        public async void ReadAsync() {
            Markup = await FetchContentAsync();
        }

        private async Task<string> FetchContentAsync() {
            using (var connection = new ImapConnection {Security = _account.ImapSecurity}) {
                using (var authenticator = await connection.ConnectAsync(_account.ImapHost, _account.ImapPort)) {
                    using (var session = await authenticator.LoginAsync(_account.ImapUsername, _account.ImapPassword)) {
                        var mailbox = await session.SelectAsync(_mailbox);
                        return await mailbox.FetchMessageBodyAsync(_envelope.Uid);
                    }
                }
            }
        }

        public bool IsBusy {
            get { return _isBusy; }
            set {
                if (_isBusy == value) {
                    return;
                }

                RaisePropertyChanging(() => IsBusy);
                _isBusy = value;
                RaisePropertyChanged(() => IsBusy);
            }
        }

        #region Implementation of IHtmlSource

        public string Markup {
            get { return _markup; }
            set {
                if (_markup == value) {
                    return;
                }
                RaisePropertyChanging(() => Markup);
                _markup = value;
                RaisePropertyChanged(() => Markup);
            }
        }

        #endregion
    }
}