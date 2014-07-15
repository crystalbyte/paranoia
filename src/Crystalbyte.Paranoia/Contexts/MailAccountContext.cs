﻿#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI.Commands;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class MailAccountContext : SelectionObject {
        private MailboxContext _selectedMailbox;
        private readonly MailAccountModel _account;
        private readonly DropMailboxCommand _dropMailboxCommand;
        private readonly ObservableCollection<MailContactContext> _contacts;
        private readonly ObservableCollection<MailboxContext> _mailboxes;

        private Exception _lastException;
        private MailContactContext _selectedContact;

        internal MailAccountContext(MailAccountModel account) {
            _account = account;
            _dropMailboxCommand = new DropMailboxCommand(this);

            _contacts = new ObservableCollection<MailContactContext>();
            _contacts.CollectionChanged +=
                (sender, e) => RaisePropertyChanged(() => Contacts);

            _mailboxes = new ObservableCollection<MailboxContext>();
        }

        public DropMailboxCommand DropMailboxCommand {
            get { return _dropMailboxCommand; }
        }

        protected override async void OnSelectionChanged() {
            base.OnSelectionChanged();

            Clear();
            if (!IsSelected)
                return;

            await UpdateAsync();
        }

        public async Task UpdateAsync() {
            await LoadContactsAsync();
            await LoadMailboxesAsync();
            await SyncMailboxesAsync();
        }

        internal void Clear() {
            _mailboxes.Clear();
            _contacts.Clear();
            _mailboxes.Clear();
        }

        internal async Task<List<ImapMailboxInfo>> ListMailboxesAsync(string pattern = "") {
            using (var connection = new ImapConnection { Security = ImapSecurity }) {
                connection.RemoteCertificateValidationFailed += (sender, e) => e.IsCanceled = false;
                using (var auth = await connection.ConnectAsync(ImapHost, ImapPort)) {
                    using (var session = await auth.LoginAsync(ImapUsername, ImapPassword)) {
                        var wildcard = string.IsNullOrEmpty(pattern) ? "%" : pattern;
                        return await session.ListAsync("", ImapMailbox.EncodeName(wildcard));
                    }
                }
            }
        }

        internal async Task SyncMailboxesAsync() {
            var mailboxes = _mailboxes.ToArray();
            var remoteMailboxes = await ListMailboxesAsync();
            if (IsGmail) {
                // Fetch gmail folders and assign automagically.
                var gmail = remoteMailboxes.FirstOrDefault(x => x.Name.ContainsIgnoreCase("gmail"));
                if (gmail != null) {
                    var pattern = string.Format("{0}{1}%", gmail.Name, gmail.Delimiter);
                    var localizedMailboxes = await ListMailboxesAsync(pattern);
                    remoteMailboxes.AddRange(localizedMailboxes);
                }
            }

            foreach (var mailbox in mailboxes) {
                if (!mailbox.IsAssigned) {
                    await mailbox.AssignMostProbableAsync(remoteMailboxes);
                }
                await mailbox.SyncAsync();
            }
        }

        internal async Task LoadMailboxesAsync() {
            var mailboxes = await Task.Factory.StartNew(() => {
                using (var context = new DatabaseContext()) {
                    context.MailAccounts.Attach(_account);
                    return _account.Mailboxes.ToArray();
                }
            });
            _mailboxes.AddRange(mailboxes.Select(x => new MailboxContext(this, x)));
            var inbox = _mailboxes.FirstOrDefault(x => x.Type == MailboxType.Inbox && x.IsAssigned);
            if (inbox != null) {
                inbox.IsSelected = true;
            }
        }

        internal async Task LoadContactsAsync() {
            var contacts = await Task.Factory.StartNew(() => {
                using (var context = new DatabaseContext()) {
                    context.MailAccounts.Attach(_account);
                    return _account.Contacts.ToArray();
                }
            });
            _contacts.AddRange(contacts.Select(x => new MailContactContext(x)));
        }

        public event EventHandler MailboxSelectionChanged;

        private async void OnMailboxSelectionChanged() {
            await HandleMailboxSelectionChangeAsync();
            var handler = MailboxSelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private async Task HandleMailboxSelectionChangeAsync() {
            var mailbox = SelectedMailbox;
            if (mailbox == null) {
                return;
            }

            mailbox.IsAssignable = !mailbox.IsAssigned;
            if (mailbox.IsAssignable) {
                await SelectedMailbox.PrepareManualAssignmentAsync();
            }

            await mailbox.LoadMessagesFromDatabaseAsync();
            var app = App.Composition.GetExport<AppContext>();
            app.DisplayMessages(SelectedMailbox.Messages);
            await mailbox.SyncAsync();
        }

        public MailboxContext SelectedMailbox {
            get { return _selectedMailbox; }
            set {
                if (_selectedMailbox == value) {
                    return;
                }

                _selectedMailbox = value;
                RaisePropertyChanged(() => SelectedMailbox);
                OnMailboxSelectionChanged();
            }
        }

        public MailContactContext SelectedContact {
            get { return _selectedContact; }
            set {
                if (_selectedContact == value) {
                    return;
                }
                _selectedContact = value;
                RaisePropertyChanged(() => SelectedContact);
            }
        }

        public string Address {
            get { return _account.Address; }
            set {
                if (_account.Address == value) {
                    return;
                }

                _account.Address = value;
                RaisePropertyChanged(() => Address);
            }
        }

        public bool IsGmail {
            get {
                return Settings.Default.GmailDomains
                    .OfType<string>()
                    .Any(x => _account.ImapHost
                        .EndsWith(x, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public string Name {
            get { return _account.Name; }
            set {
                if (_account.Name == value) {
                    return;
                }

                _account.Name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public string ImapHost {
            get { return _account.ImapHost; }
            set {
                if (_account.ImapHost == value) {
                    return;
                }

                _account.ImapHost = value;
                RaisePropertyChanged(() => ImapHost);
            }
        }

        public short ImapPort {
            get { return _account.ImapPort; }
            set {
                if (_account.ImapPort == value) {
                    return;
                }

                _account.ImapPort = value;
                RaisePropertyChanged(() => ImapPort);
            }
        }

        public string ImapUsername {
            get { return _account.ImapUsername; }
            set {
                if (_account.ImapUsername == value) {
                    return;
                }

                _account.ImapUsername = value;
                RaisePropertyChanged(() => ImapUsername);
            }
        }

        public string ImapPassword {
            get { return _account.ImapPassword; }
            set {
                if (_account.ImapPassword == value) {
                    return;
                }

                _account.ImapPassword = value;
                RaisePropertyChanged(() => ImapPassword);
            }
        }

        public SecurityPolicy ImapSecurity {
            get { return _account.ImapSecurity; }
            set {
                if (_account.ImapSecurity == value) {
                    return;
                }

                _account.ImapSecurity = value;
                RaisePropertyChanged(() => ImapSecurity);
            }
        }

        public Exception LastException {
            get { return _lastException; }
            set {
                if (_lastException == value) {
                    return;
                }

                _lastException = value;
                RaisePropertyChanged(() => LastException);
            }
        }

        public IEnumerable<MailContactContext> Contacts {
            get { return _contacts; }
        }

        public IEnumerable<MailboxContext> Mailboxes {
            get { return _mailboxes; }
        }
    }
}