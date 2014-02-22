#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Models;
using Crystalbyte.Paranoia.Messaging.Mime;
using System.Text;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class MailContext : NotificationObject {

        private string _text;
        private readonly List<MailContactContext> _mailContacts;
        private readonly MailboxContext _mailbox;
        private readonly Mail _mail;
        private bool _isSelected;

        internal MailContext(MailboxContext mailbox, Mail mail) {
            _mail = mail;
            _mailbox = mailbox;
            _mailContacts = new List<MailContactContext>();
        }

        public int Id {
            get { return _mail.Id; }
        }

        public DateTime? Date {
            get { return _mail.InternalDate; }
        }

        public long Size {
            get { return _mail.Size; }
        }

        public string Subject {
            get { return _mail.Subject; }
        }

        public long Uid {
            get { return _mail.Uid; }
        }

        public string Text {
            get { return _text; }
            set {
                if (_text == value) {
                    return;
                }

                RaisePropertyChanging(() => Text);
                _text = value;
                RaisePropertyChanged(() => Text);
            }
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

        private async void OnSelected() {
            await DisplayMailAsync();
        }

        public MailContactContext FromFirst {
            get {
                return _mailContacts.FirstOrDefault(x => x.Type == MailContactType.From);
            }
        }

        internal async Task RestoreContactsAsync() {
            using (var context = new StorageContext()) {
                context.Mails.Attach(_mail);
                IEnumerable<MailContact> contacts = null;
                await Task.Factory.StartNew(() => {
                    contacts = _mail.MailContacts.ToArray();
                });

                if (contacts == null) {
                    return;
                }

                _mailContacts.AddRange(contacts.Select(x => new MailContactContext(this, x)));
            }
        }

        private async Task DisplayMailAsync() {
            var account = _mailbox.ImapAccount;
            using (var connection = new ImapConnection { Security = account.Security }) {
                using (var authenticator = await connection.ConnectAsync(account.Host, account.Port)) {
                    using (var session = await authenticator.LoginAsync(account.Username, account.Password)) {
                        var mailbox = await session.SelectAsync(_mailbox.Name);
                        var content = await mailbox.FetchMessageBodyAsync(_mail.Uid);

                        var bytes = Encoding.UTF8.GetBytes(content);
                        var message = new MailMessage(bytes);

                        var html = message.FindFirstHtmlVersion();
                        Text = Encoding.UTF8.GetString(html.Body);
                    }
                }
            }
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