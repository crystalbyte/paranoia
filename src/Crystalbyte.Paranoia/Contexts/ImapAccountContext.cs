#region Using directives

using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class ImapAccountContext : NotificationObject {

        #region Private Fields

        private bool _isSelected;
        private readonly ImapAccount _account;
        private readonly ObservableCollection<MailboxContext> _mailboxes;

        #endregion

        #region Construction

        public ImapAccountContext(ImapAccount account) {
            _account = account;
            _mailboxes = new ObservableCollection<MailboxContext>();
        }

        #endregion

        public ObservableCollection<MailboxContext> Mailboxes {
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

        public SecurityPolicy Security {
            get { return (SecurityPolicy)_account.Security; }
            set {
                if (_account.Security == value) {
                    return;
                }

                RaisePropertyChanging(() => Security);
                _account.Security = value;
                RaisePropertyChanged(() => Security);
            }
        }

        public string Host {
            get { return _account.Host; }
            set {
                if (_account.Host == value) {
                    return;
                }

                RaisePropertyChanging(() => Host);
                _account.Host = value;
                RaisePropertyChanged(() => Host);
            }
        }

        public short ImapPort {
            get { return _account.Port; }
            set {
                if (_account.Port == value) {
                    return;
                }

                RaisePropertyChanging(() => ImapPort);
                if (value < 0) {
                    value = 0;
                }
                if (value > short.MaxValue) {
                    value = short.MaxValue;
                }
                _account.Port = value;
                RaisePropertyChanged(() => ImapPort);
            }
        }

        public string ImapUsername {
            get { return _account.Username; }
            set {
                if (_account.Username == value) {
                    return;
                }

                RaisePropertyChanging(() => ImapUsername);
                _account.Username = value;
                RaisePropertyChanged(() => ImapUsername);
            }
        }

        public string Password {
            get { return _account.Password; }
            set {
                if (_account.Password == value) {
                    return;
                }

                RaisePropertyChanging(() => Password);
                _account.Password = value;
                RaisePropertyChanged(() => Password);
            }
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

        //internal async Task LoadMailboxesAsync() {
        //    var mailboxes = _account.Mailboxes.Select(x => new MailboxContext(this, x));
        //    Mailboxes.AddRange(mailboxes);

        //    if (_account.Mailboxes.Count > 0) {
        //        return;
        //    }

        //    await RequestMailboxesAsync();
        //    mailboxes = _account.Mailboxes.Select(x => new MailboxContext(this, x));
        //    Mailboxes.AddRange(mailboxes);
        //}

        private async Task RequestMailboxesAsync() {
            using (var connection = new ImapConnection { Security = Security }) {
                using (var authenticator = await connection.ConnectAsync(Host, ImapPort)) {
                    using (var session = await authenticator.LoginAsync(ImapUsername, Password)) {
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
            _account.Mailboxes.Add(new Mailbox {
                Name = info.Fullname,
                Delimiter = info.Delimiter,
                Flags = info.Flags.Select(x => new MailboxFlag { Name = x }).ToList()
            });
            await Task.Factory.StartNew(() => {
                try {
                                                                
                } catch (Exception ex) {
                    ErrorLogContext.Current.PushError(ex);
                }
            });
        }

        internal async Task TakeOnlineAsync() {
            //await LoadMailboxesAsync();
        }
    }
}