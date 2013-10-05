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
            Accounts = new ObservableCollection<AccountContext>();
        }

        public event EventHandler SyncStatusChanged;

        public void OnSyncStatusChanged(EventArgs e) {
            var handler = SyncStatusChanged;
            if (handler != null)
                handler(this, e);
        }

        [Import]
        public AccountContextFactory AccountContextFactory { get; set; }

        [Import]
        public ComposeMessageCommand ComposeMessageCommand { get; set; }

        [Import]
        public SyncCommand SyncCommand { get; set; }

        public ObservableCollection<AccountContext> Accounts { get; set; }

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
            var account = new Account {
                Host = "imap.gmail.com",
                Port = 993,
                Username = "paranoia.app@gmail.com",
                Password = "p4r4n014"
            };
            Accounts.Add(AccountContextFactory.Create(account));
        }

        public IEnumerable<MessageContext> Messages {
            get { return Accounts.SelectMany(x => x.Messages); }
        }

        public async Task SyncAsync() {
            IsSyncing = true;

            Accounts.ForEach(x => x.Messages.Clear());

            foreach (var account in Accounts) {
                await account.SyncAsync();
            }

            RaisePropertyChanged(() => Messages);
            IsSyncing = false;
        }
    }
}
