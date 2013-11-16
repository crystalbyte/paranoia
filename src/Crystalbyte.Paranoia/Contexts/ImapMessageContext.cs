﻿#region Using directives

using System;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Models;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class ImapMessageContext : NotificationObject, IHtmlSource {
        private readonly ImapEnvelope _envelope;
        private readonly ImapAccountContext _account;
        private readonly string _mailbox;
        private string _markup;
        private bool _isBusy;

        public ImapMessageContext(ImapAccountContext account, ImapEnvelope envelope, string mailbox) {
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
            get { return string.Format("{0}@{1}:{2}", _account.Username, _account.Host, _mailbox); }
        }

        public ImapAccountContext Account {
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
            using (var connection = new ImapConnection { Security = _account.Security }) {
                using (var authenticator = await connection.ConnectAsync(_account.Host, _account.Port)) {
                    using (var session = await authenticator.LoginAsync(_account.Username, _account.Password)) {
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