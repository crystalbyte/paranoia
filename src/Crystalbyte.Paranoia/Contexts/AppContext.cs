#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Commands;
using Crystalbyte.Paranoia.Contexts.Factories;
using Crystalbyte.Paranoia.Models;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    [Export, Shared]
    public sealed class AppContext : NotificationObject {
        private bool _isSyncing;
        private string _messageBody;

        public AppContext() {
            ImapAccounts = new ObservableCollection<ImapAccountContext>();
            SmtpAccounts = new ObservableCollection<SmtpAccountContext>();
        }

        public event EventHandler SyncStatusChanged;

        public void OnSyncStatusChanged(EventArgs e) {
            var handler = SyncStatusChanged;
            if (handler != null)
                handler(this, e);
        }

        [Import]
        public ImapAccountContextFactory ImapAccountContextFactory { get; set; }

        [Import]
        public SmtpAccountContextFactory SmtpAccountContextFactory { get; set; }

        [Import]
        public ComposeMessageCommand ComposeMessageCommand { get; set; }

        [Import]
        public SyncCommand SyncCommand { get; set; }

        public ObservableCollection<ImapAccountContext> ImapAccounts { get; private set; }
        public ObservableCollection<SmtpAccountContext> SmtpAccounts { get; private set; }

        public string MessageBody {
            get { return _messageBody; }
            set {
                if (_messageBody == value) {
                    return;
                }

                RaisePropertyChanging(() => MessageBody);
                _messageBody = value;
                RaisePropertyChanged(() => MessageBody);
            }
        }

        public bool IsSyncing {
            get { return _isSyncing; }
            set {
                if (_isSyncing == value) {
                    return;
                }

                RaisePropertyChanging(() => IsSyncing);
                _isSyncing = value;
                RaisePropertyChanged(() => IsSyncing);
                OnSyncStatusChanged(EventArgs.Empty);
            }
        }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            var imap = new ImapAccount {
                Host = "imap.gmail.com",
                Port = 993,
                Username = "paranoia.app@gmail.com",
                Password = "p4r4n014"
            };

            ImapAccounts.Add(ImapAccountContextFactory.Create(imap));

            var smtp = new SmtpAccount {
                Host = "smtp.gmail.com",
                Port = 587,
                Username = "paranoia.app@gmail.com",
                Password = "p4r4n014"
            };

            SmtpAccounts.Add(SmtpAccountContextFactory.Create(smtp));
        }

        public IEnumerable<ImapEnvelopeContext> Messages {
            get { return ImapAccounts.SelectMany(x => x.Messages); }
        }

        public async Task SyncAsync() {
            IsSyncing = true;

            ImapAccounts.ForEach(x => x.Messages.Clear());

            foreach (var account in ImapAccounts) {
                await account.SyncAsync();
            }

            RaisePropertyChanged(() => Messages);
            IsSyncing = false;
        }
    }
}
