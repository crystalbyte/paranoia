#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Models;
using System.Text;
using NLog;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class MailContext : NotificationObject {

        private string _text;
        private readonly List<MailFlagContext> _mailFlags;
        private readonly List<MailContactContext> _mailContacts;
        private readonly MailboxContext _mailbox;
        private readonly Mail _mail;
        private bool _isSelected;

        #region Log Declaration

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        internal MailContext(MailboxContext mailbox, Mail mail) {
            _mail = mail;
            _mailbox = mailbox;
            _mailContacts = new List<MailContactContext>();
            _mailFlags = new List<MailFlagContext>();
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

        public bool IsDeleted {
            get { return _mailFlags.Any(x => x.Name.ContainsIgnoreCase(@"\Deleted")); }
        }

        public bool IsSeen {
            get { return _mailFlags.Any(x => x.Name.ContainsIgnoreCase(@"\Seen")); }
        }

        public bool IsFlagged {
            get { return _mailFlags.Any(x => x.Name.ContainsIgnoreCase(@"\Flagged")); }
        }

        public bool IsImportant {
            get { return _mailFlags.Any(x => x.Name.ContainsIgnoreCase(@"\Important")); }
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

        private async Task RestoreContactsAsync() {
            IEnumerable<MailContact> contacts = null;
            await Task.Factory.StartNew(() => {
                contacts = _mail.MailContacts.ToArray();
            });
            if (contacts == null) {
                return;
            }
            _mailContacts.AddRange(contacts.Select(x => new MailContactContext(this, x)));
        }

        internal async Task RestoreAsync(StorageContext context = null) {
            var dispose = false;
            if (context == null) {
                context = new StorageContext();
                context.Mails.Attach(_mail);
                dispose = true;
            }

            await RestoreContactsAsync();
            await RestoreFlagsAsync();

            if (dispose) {
                context.Dispose();
            }

            RaisePropertyChanged(string.Empty);
        }

        internal async Task DeleteAsync() {
            var account = _mailbox.ImapAccount;
            _mailbox.Mails.Remove(this);

            using (var connection = new ImapConnection { Security = account.Security }) {
                using (var authenticator = await connection.ConnectAsync(account.Host, account.Port)) {
                    using (var session = await authenticator.LoginAsync(account.Username, account.Password)) {
                        var box = await session.SelectAsync(_mailbox.Name);
                        try {
                            await DeleteCachedMailAsync();
                            await box.DeleteMailsAsync(new [] { Uid });
                        } catch (Exception ex) {
                            Log.Error(ex.Message);
                        }
                    }
                }
            }
        }

        private async Task DeleteCachedMailAsync() {
            try {
                using (var context = new StorageContext()) {
                    var mail = await context.Mails.FindAsync(Id);
                    if (mail != null) {
                        context.Mails.Remove(mail);
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex) {
                Log.Error(ex.Message);
            }
        }

        private async Task RestoreFlagsAsync() {
            IEnumerable<MailFlag> flags = null;
            await Task.Factory.StartNew(() => {
                flags = _mail.MailFlags.ToArray();
            });
            if (flags == null) {
                return;
            }
            _mailFlags.AddRange(flags.Select(x => new MailFlagContext(this, x)));
        }

        private async Task DisplayMailAsync() {
            var account = _mailbox.ImapAccount;
            using (var connection = new ImapConnection { Security = account.Security }) {
                using (var authenticator = await connection.ConnectAsync(account.Host, account.Port)) {
                    using (var session = await authenticator.LoginAsync(account.Username, account.Password)) {
                        var mailbox = await session.SelectAsync(_mailbox.Name);
                        var content = await mailbox.FetchMessageBodyAsync(_mail.Uid);
                        if (string.IsNullOrWhiteSpace(content)) {
                            // Mail has been deleted.
                            await DeleteAsync();
                            return;
                        }

                        var bytes = Encoding.UTF8.GetBytes(content);
                        var message = new MailMessage(bytes);

                        var html = message.FindFirstHtmlVersion();
                        Text = Encoding.UTF8.GetString(html.Body);
                    }
                }
            }
        }
    }
}