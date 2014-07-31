#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Packaging;
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
        private readonly AppContext _appContext;
        private readonly MailAccountModel _account;
        private readonly DropAssignmentCommand _dropMailboxCommand;
        private readonly ObservableCollection<MailContactContext> _contacts;
        private readonly ObservableCollection<MailboxContext> _mailboxes;

        private Exception _lastException;
        private MailContactContext _selectedContact;

        internal MailAccountContext(MailAccountModel account, AppContext appContext) {
            _account = account;
            _appContext = appContext;
            _dropMailboxCommand = new DropAssignmentCommand(this);

            _contacts = new ObservableCollection<MailContactContext>();
            _contacts.CollectionChanged += (sender, e) => RaisePropertyChanged(() => Contacts);

            _mailboxes = new ObservableCollection<MailboxContext>();
        }

        public DropAssignmentCommand DropMailboxCommand {
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


            var t1 = mailboxes
                .Where(x => !x.IsAssigned)
                .Select(x => x.AssignMostProbableAsync(remoteMailboxes));

            await Task.WhenAll(t1);

            var t2 = mailboxes
                .Where(x => x.IsAssigned)
                .Select(x => x.SyncAsync());

            await Task.WhenAll(t2);
        }

        internal async Task LoadMailboxesAsync() {
            var mailboxes = await Task.Factory.StartNew(() => {
                using (var context = new DatabaseContext()) {
                    context.MailAccounts.Attach(_account);
                    return _account.Mailboxes.ToArray();
                }
            });
            _mailboxes.AddRange(mailboxes.Select(x => new MailboxContext(this, x)));
            var inbox = _mailboxes.FirstOrDefault(x => x.Type == MailboxType.Inbox);
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
            if (_contacts.Count > 0) {
                _contacts.First().IsSelected = true;
            }
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

            await mailbox.UpdateAsync(SelectedContact);
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
                OnSelectedContactChanged();
            }
        }

        private async void OnSelectedContactChanged() {
            if (SelectedMailbox == null) {
                SelectedMailbox = Mailboxes.FirstOrDefault(x => x.Type == MailboxType.Inbox);
            }

            if (SelectedMailbox == null) {
                _appContext.ClearMessages();
                return;
            }

            await SelectedMailbox.UpdateAsync(SelectedContact);
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

        public Int64 Id {
            get { return _account.Id; }
        }

        public bool IsGmail {
            get {
                return Settings.Default.GmailDomains
                    .OfType<string>()
                    .Any(x => _account.ImapHost
                        .EndsWith(x, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public AppContext AppContext {
            get { return _appContext; }
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