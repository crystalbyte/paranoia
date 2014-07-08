using System;
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
        private MailboxContext _selectedMailbox;
        private object _messagesSource;

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
            MailAccountSelectionSource.SelectionChanged += OnAccountSelectionChanged;
        }

        #endregion
        private void OnAccountSelectionChanged(object sender, EventArgs e) {
            SelectedAccount = MailAccountSelectionSource.Selection.FirstOrDefault();
        }

        public void UpdateMessages() {
            MessagesSource = MailboxSelectionSource.Selection
                .SelectMany(x => x.Messages)
                .ToArray();
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
                RaisePropertyChanged(() => SelectedAccount);
            }
        }

        public async Task RunAsync() {
            await LoadAccountsAsync();
            if (Accounts.Count > 0) {
                MailAccountSelectionSource.Selection = new[] { Accounts.First() };
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
