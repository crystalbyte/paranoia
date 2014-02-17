#region Using directives

using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class ImapAccountContext : NotificationObject {

        #region Private Fields

        private readonly ImapAccount _account;
        private readonly ObservableCollection<MailboxContext> _mailboxes;

        #endregion

        #region Construction

        public ImapAccountContext(ImapAccount account) {
            _account = account;
            _mailboxes = new ObservableCollection<MailboxContext>();
        }

        #endregion

        #region Log Declaration

        private static Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        public ObservableCollection<MailboxContext> Mailboxes {
            get { return _mailboxes; }
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

        public short Port {
            get { return _account.Port; }
            set {
                if (_account.Port == value) {
                    return;
                }

                RaisePropertyChanging(() => Port);
                if (value < 0) {
                    value = 0;
                }
                if (value > short.MaxValue) {
                    value = short.MaxValue;
                }
                _account.Port = value;
                RaisePropertyChanged(() => Port);
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

        private async Task RestoreMailboxesAsync() {
            MailboxContext[] mailboxes = null;
            await Task.Factory.StartNew(() => {
                try {
                    using (var context = new StorageContext()) {
                        var account = context.ImapAccounts.First(x => x.IdentityId == _account.IdentityId);
                        mailboxes = account.Mailboxes.ToArray()
                            .Select(x => new MailboxContext(this, x)).ToArray();
                    }
                } catch (Exception ex) {
                    Log.Error(ex.Message);
                }
            });

            Mailboxes.Clear();
            Mailboxes.AddRange(mailboxes);
        }

        internal async Task SyncMailboxesAsync() {
            await RestoreMailboxesAsync();
            await FetchMailboxesAsync();
            
        }

        private async Task FetchMailboxesAsync() {
            using (var connection = new ImapConnection { Security = Security }) {
#if DEBUG
                connection.RemoteCertificateValidationFailed += (sender, e) => e.IsCancelled = false;
#endif
                using (var authenticator = await connection.ConnectAsync(Host, Port)) {
                    using (var session = await authenticator.LoginAsync(ImapUsername, Password)) {
                        var mailboxes = (await session.ListAsync("", "*")).ToArray();

                        var inbox = mailboxes.FirstOrDefault(x => x.IsInbox);
                        if (inbox != null && !Mailboxes.Any(x => x.Name == inbox.Name)) {
                            await SaveMailboxAsync(inbox);
                        }

                        var flagged = mailboxes.FirstOrDefault(x => x.IsFlagged);
                        if (flagged != null && !Mailboxes.Any(x => x.IsFlagged)) {
                            await SaveMailboxAsync(flagged);
                        }

                        var important = mailboxes.FirstOrDefault(x => x.IsImportant);
                        if (important != null && !Mailboxes.Any(x => x.IsImportant)) {
                            await SaveMailboxAsync(important);
                        }

                        var trash = mailboxes.FirstOrDefault(x => x.IsTrash);
                        if (trash != null && !Mailboxes.Any(x => x.IsTrash)) {
                            await SaveMailboxAsync(trash);
                        }

                        var sent = mailboxes.FirstOrDefault(x => x.IsSent);
                        if (sent != null && !Mailboxes.Any(x => x.IsSent)) {
                            await SaveMailboxAsync(sent);
                        }

                        var draft = mailboxes.FirstOrDefault(x => x.IsDraft);
                        if (draft != null && !Mailboxes.Any(x => x.IsDraft)) {
                            await SaveMailboxAsync(draft);
                        }
                    }
                }
            }
        }

        private async Task SaveMailboxAsync(ImapMailboxInfo info) {
            var mailbox = new Mailbox {
                Name = info.Fullname,
                Delimiter = info.Delimiter,
                Flags = info.Flags.Select(x => new MailboxFlag { Name = x }).ToList()
            };
            await Task.Factory.StartNew(() => {
                try {
                    using (var context = new StorageContext()) {
                        var account = context.ImapAccounts.First(x => x.IdentityId == _account.IdentityId);
                        account.Mailboxes.Add(mailbox);
                        context.SaveChanges();
                    }                                                   
                } catch (Exception ex) {
                    Log.Error(ex.Message);
                }
            });
        }
    }
}