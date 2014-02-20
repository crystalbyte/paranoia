#region Using directives

using System;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Models;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class MailContext : NotificationObject {

        private readonly Mail _mail;
        private bool _isSelected;

        internal MailContext(Mail mail) {
            _mail = mail;
        }

        public DateTime? Date {
            get { return _mail.InternalDate; }
        }

        public string Subject {
            get { return _mail.Subject; }
        }

        public long Uid {
            get { return _mail.Uid; }
        }

        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) {
                    return;
                }

                RaisePropertyChanging(() => IsSelected);
                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);

                if (value) {
                    OnSelected();
                }
            }
        }

        private void OnSelected() {
            
        }

        //public bool IsSeen {
        //    get { return _mail.Flags.Any(x => x.ContainsIgnoreCase("\\Seen")); }
        //}

        public async void ReadAsync() {
            //Markup = await FetchContentAsync();
        }

        //private async Task<string> FetchContentAsync() {
        //    using (var connection = new ImapConnection {Security = _account.Security}) {
        //        using (var authenticator = await connection.ConnectAsync(_account.Host, _account.ImapPort)) {
        //            using (var session = await authenticator.LoginAsync(_account.ImapUsername, _account.Password)) {
        //                var mailbox = await session.SelectAsync(_mailbox);
        //                return await mailbox.FetchMessageBodyAsync(_envelope.Uid);
        //            }
        //        }
        //    }
        //}

        #region Implementation of IHtmlSource

        //public string Markup {
        //    get { return _markup; }
        //    set {
        //        if (_markup == value) {
        //            return;
        //        }
        //        RaisePropertyChanging(() => Markup);
        //        _markup = value;
        //        RaisePropertyChanged(() => Markup);
        //    }
        //}

        #endregion
    }
}