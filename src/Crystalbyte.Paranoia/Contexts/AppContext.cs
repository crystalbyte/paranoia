using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;

namespace Crystalbyte.Paranoia {

    [Export, Shared]
    public sealed class AppContext : NotificationObject {
        private MailAccountContext _selectedAccount;
        private IEnumerable<MailboxContext> _selectedMailboxes;
        private object _messagesSource;
        private MailboxContext _selectedMailbox;


        public AppContext() {
            Accounts = new ObservableCollection<MailAccountContext>();
        }

        public ObservableCollection<MailAccountContext> Accounts { get; set; }

        #region Import Directives

        [Import]
        public MailboxSelectionSource MailboxSelectionSource { get; set; }

        [Import]
        public MailAccountSelectionSource MailAccountSelectionSource { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            MailboxSelectionSource.SelectionChanged += OnMailboxSelectionChanged;
            MailAccountSelectionSource.SelectionChanged += OnAccountSelectionChanged;
        }

        private async void OnMailboxSelectionChanged(object sender, EventArgs e) {
            MessagesSource = null;
            var selection = MailboxSelectionSource.Selection.ToArray();
            SelectedMailbox = selection.Length == 1
                ? selection[0]
                : null;

            if (SelectedMailbox != null) {
                SelectedMailbox.IsMailboxAssignable 
                    = SelectedMailbox != null && !SelectedMailbox.IsAssigned;
                if (SelectedMailbox.IsMailboxAssignable) {
                    await SelectedMailbox.PrepareManualAssignmentAsync();
                }
            }

            SelectedMailboxes = selection;
            await UpdateMessageViewAsync();
        }

        #endregion
        private void OnAccountSelectionChanged(object sender, EventArgs e) {
            SelectedAccount = MailAccountSelectionSource.Selection.FirstOrDefault();
        }

        public async Task UpdateMessageViewAsync() {
            if (MailboxSelectionSource.Selection == null) {
                MessagesSource = null;
                return;
            }

            var mailboxes = MailboxSelectionSource.Selection.Where(x => x.IsAssigned).ToArray();
            foreach (var mailbox in mailboxes.AsParallel()) {
                await mailbox.LoadMessagesFromDatabaseAsync();
            }

            // Show cached messages
            MessagesSource = mailboxes
                .SelectMany(x => x.Messages.ToArray())
                .ToArray();

            // Sync with server

            foreach (var mailbox in mailboxes.AsParallel()) {
                await mailbox.SyncAsync();
            }

        }

        public object MessagesSource {
            get { return _messagesSource; }
            set {
                if (_messagesSource == value) {
                    return;
                }

                _messagesSource = value;
                RaisePropertyChanged(() => MessagesSource);
            }
        }

        public MailAccountContext SelectedAccount {
            get { return _selectedAccount; }
            set {
                if (_selectedAccount == value) {
                    return;
                }

                _selectedAccount = value;
                RaisePropertyChanged(() => SelectedAccount);
            }
        }

        public MailboxContext SelectedMailbox {
            get { return _selectedMailbox; }
            set {
                if (_selectedMailbox == value) {
                    return;
                }
                _selectedMailbox = value;
                RaisePropertyChanged(() => SelectedMailbox);
            }
        }

        public IEnumerable<MailboxContext> SelectedMailboxes {
            get { return _selectedMailboxes; }
            set {
                if (Equals(_selectedMailboxes, value)) {
                    return;
                }

                _selectedMailboxes = value;
                RaisePropertyChanged(() => SelectedAccount);
            }
        }

        public async Task RunAsync() {
            await LoadAccountsAsync();
            if (Accounts.Count > 0) {
                Accounts.First().IsSelected = true;
                //MailAccountSelectionSource.Selection = new[] { Accounts.First() };
            }
        }

        private async Task LoadAccountsAsync() {
            using (var context = new DatabaseContext()) {
                var accounts = await context.MailAccounts.ToArrayAsync();
                Accounts.AddRange(accounts.Select(x => new MailAccountContext(x)));
            }
        }
    }
}
