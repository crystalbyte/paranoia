#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI.Commands;
using MailMessage = System.Net.Mail.MailMessage;
using System.IO;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class MailAccountContext : SelectionObject {
        private MailboxContext _selectedMailbox;
        private readonly AppContext _appContext;
        private readonly MailAccountModel _account;
        private readonly DropAssignmentCommand _dropMailboxCommand;
        private readonly ObservableCollection<MailContactContext> _contacts;
        private readonly ObservableCollection<MailboxContext> _mailboxes;
        private MailContactContext _selectedContact;
        private bool _sendingMessages;

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

        public string SmtpHost {
            get { return _account.SmtpHost; }
            set {
                if (_account.SmtpHost == value) {
                    return;
                }

                _account.SmtpHost = value;
                RaisePropertyChanged(() => SmtpHost);
            }
        }

        public short SmtpPort {
            get { return _account.SmtpPort; }
            set {
                if (_account.SmtpPort == value) {
                    return;
                }

                _account.SmtpPort = value;
                RaisePropertyChanged(() => SmtpPort);
            }
        }

        public string SmtpUsername {
            get { return _account.SmtpUsername; }
            set {
                if (_account.SmtpUsername == value) {
                    return;
                }

                _account.SmtpUsername = value;
                RaisePropertyChanged(() => SmtpUsername);
            }
        }

        public string SmtpPassword {
            get { return _account.SmtpPassword; }
            set {
                if (_account.SmtpPassword == value) {
                    return;
                }

                _account.SmtpPassword = value;
                RaisePropertyChanged(() => SmtpPassword);
            }
        }

        public SecurityPolicy SmtpSecurity {
            get { return _account.SmtpSecurity; }
            set {
                if (_account.SmtpSecurity == value) {
                    return;
                }

                _account.SmtpSecurity = value;
                RaisePropertyChanged(() => SmtpSecurity);
            }
        }

        public IEnumerable<MailContactContext> Contacts {
            get { return _contacts; }
        }

        public IEnumerable<MailboxContext> Mailboxes {
            get { return _mailboxes; }
        }

        internal async Task ProcessOutgoingMessagesAsync() {
            var requests = await GetPendingSmtpRequestsAsync();
            if (!requests.Any() || _sendingMessages) {
                return;
            }

            _sendingMessages = true;

            var sheet = await GetHtmlCoverSheetAsync();
            sheet = sheet.Replace("%FROM%", _account.Name);

            foreach (var request in requests) {
                try {
                    // TODO: zip mime before encrypting for optimal compression (request.Mime)

                    // TODO: Fetch public keys, abort if no connection, cant send anyways.
                    // TODO: foreach public key => do {
                    // TODO: encrypt compressed mime

                    using (var connection = new SmtpConnection { Security = SmtpSecurity }) {
                        using (var auth = await connection.ConnectAsync(SmtpHost, SmtpPort)) {
                            using (var session = await auth.LoginAsync(SmtpUsername, SmtpPassword)) {

                                var wrapper = new MailMessage(
                                    new MailAddress(_account.Address, _account.Name),
                                    new MailAddress(request.ToAddress)) {
                                        Subject = string.Format(Resources.SubjectTemplate, _account.Name),
                                        Body = sheet,
                                        IsBodyHtml = true,
                                        BodyEncoding = Encoding.UTF8,
                                        HeadersEncoding = Encoding.UTF8,
                                        SubjectEncoding = Encoding.UTF8,
                                        BodyTransferEncoding = TransferEncoding.Base64,
                                    };

                                var guid = Guid.NewGuid();
                                using (var writer = new StreamWriter(new MemoryStream()) { AutoFlush = true }) {
                                    await writer.WriteAsync(request.Mime);
                                    writer.BaseStream.Seek(0, SeekOrigin.Begin);
                                    wrapper.Attachments.Add(new Attachment(writer.BaseStream, guid.ToString()) {
                                        TransferEncoding = TransferEncoding.Base64,
                                        NameEncoding = Encoding.UTF8
                                    });
                                    await session.SendAsync(wrapper);
                                }
                            }
                        }
                    }

                    // TODO: foreach public key => end }

                    await DropRequestFromDatabaseAsync(request);

                } catch (Exception) {
                    throw;
                } finally {
                    _sendingMessages = false;
                }
            }
        }

        private static Task DropRequestFromDatabaseAsync(SmtpRequestModel request) {
            using (var database = new DatabaseContext()) {
                database.SmtpRequests.Attach(request);
                database.SmtpRequests.Remove(request);
                return database.SaveChangesAsync();
            }
        }

        private static Task<string> GetHtmlCoverSheetAsync() {
            const string name = "/Resources/cover.sheet.template.html";
            var info = Application.GetResourceStream(new Uri(name, UriKind.Relative));
            if (info == null) {
                var message = string.Format(Resources.ResourceNotFoundException, name, typeof(App).Name);
                throw new Exception(message);
            }

            using (var reader = new StreamReader(info.Stream)) {
                return reader.ReadToEndAsync();
            }
        }

        private static Task<SmtpRequestModel[]> GetPendingSmtpRequestsAsync() {
            using (var database = new DatabaseContext()) {
                return database.SmtpRequests.ToArrayAsync();
            }
        }

        internal async Task SaveSmtpRequestsAsync(IEnumerable<MailMessage> messages) {
            using (var database = new DatabaseContext()) {
                var account = await database.MailAccounts.FindAsync(_account.Id);

                foreach (var message in messages) {
                    var request = new SmtpRequestModel {
                        ToAddress = message.To.First().Address,
                        Mime = await message.ToMimeAsync()
                    };

                    account.SmtpRequests.Add(request);
                }

                await database.SaveChangesAsync();
            }
        }
    }
}