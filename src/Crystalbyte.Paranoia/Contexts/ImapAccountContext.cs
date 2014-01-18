#region Using directives

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Models;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Crystalbyte.Paranoia.Data;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class ImapAccountContext : NotificationObject {

        #region Private Fields

        private readonly ImapAccount _imap;

        private bool _isSelected;
        private readonly ObservableCollection<ImapMailboxContext> _mailboxes;

        #endregion

        #region Construction

        public ImapAccountContext(ImapAccount imap) {
            _imap = imap;
            _mailboxes = new ObservableCollection<ImapMailboxContext>();
        }

        #endregion

        public ObservableCollection<ImapMailboxContext> Mailboxes {
            get { return _mailboxes; }
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
            }
        }

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

        internal async Task LoadMailboxesAsync() {
            var mailboxes = _imap.Mailboxes.Select(x => new ImapMailboxContext(this, x));
            Mailboxes.AddRange(mailboxes);

            if (_imap.Mailboxes.Count > 0) {
                return;
            }

            await RequestMailboxesAsync();
            mailboxes = _imap.Mailboxes.Select(x => new ImapMailboxContext(this, x));
            Mailboxes.AddRange(mailboxes);
        }

        private async Task RequestMailboxesAsync() {
            using (var connection = new ImapConnection { Security = ImapSecurity }) {
                using (var authenticator = await connection.ConnectAsync(ImapHost, ImapPort)) {
                    using (var session = await authenticator.LoginAsync(ImapUsername, ImapPassword)) {
                        var mailboxes = (await session.ListAsync("", "*")).ToArray();

                        var inbox = mailboxes.FirstOrDefault(x => x.IsInbox);
                        if (inbox != null) {
                            await SaveMailboxAsync(inbox);
                        }

                        var all = mailboxes.FirstOrDefault(x => x.IsAll);
                        if (all != null) {
                            await SaveMailboxAsync(all);
                        }

                        var important = mailboxes.FirstOrDefault(x => x.IsImportant);
                        if (important != null) {
                            await SaveMailboxAsync(important);
                        }

                        var trash = mailboxes.FirstOrDefault(x => x.IsTrash);
                        if (trash != null) {
                            await SaveMailboxAsync(trash);
                        }

                        var sent = mailboxes.FirstOrDefault(x => x.IsSent);
                        if (sent != null) {
                            await SaveMailboxAsync(sent);
                        }

                        var draft = mailboxes.FirstOrDefault(x => x.IsDraft);
                        if (draft != null) {
                            await SaveMailboxAsync(draft);
                        }
                    }
                }
            }
        }

        private async Task SaveMailboxAsync(ImapMailboxInfo info) {
            _imap.Mailboxes.Add(new Mailbox {
                Fullname = info.Fullname,
                Delimiter = info.Delimiter.ToString(CultureInfo.InvariantCulture),
                MailboxFlags = info.Flags.Select(x => new MailboxFlag { Name = x }).ToArray()
            });
            await Task.Factory.StartNew(() => {
                try {
                    LocalStorage.Current.Context.SaveChanges();
                }
                catch (Exception ex) {
                    ErrorLogContext.Current.PushError(ex);
                }
            });
        }

        internal async Task TakeOnlineAsync() {
            await LoadMailboxesAsync();
        }
    }
}