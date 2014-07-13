using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using System.Text;
using Crystalbyte.Paranoia.UI.Commands;

namespace Crystalbyte.Paranoia {

    [Export, Shared]
    public sealed class AppContext : NotificationObject {

        private MailAccountContext _selectedAccount;
        private IEnumerable<MailMessageContext> _selectedMessages;
        private readonly ObservableCollection<MailAccountContext> _accounts;
        private readonly ReplyCommand _replyCommand;
        private readonly DeleteCommand _deleteCommand;
        private readonly ForwardCommand _forwardCommand;
        private Exception _lastException;
        private string _queryString;
        private object _messages;
        private string _html;

        public AppContext() {
            _accounts = new ObservableCollection<MailAccountContext>();
            _replyCommand = new ReplyCommand(this);
            _forwardCommand = new ForwardCommand(this);
            _deleteCommand = new DeleteCommand(this);
        }

        public IEnumerable<MailAccountContext> Accounts {
            get { return _accounts; }
        }

        public event EventHandler MessageSelectionChanged;

        private async void OnMessageSelectionChanged() {
            ClearMessageView();
            var message = SelectedMessages.FirstOrDefault();
            if (message == null) {
                return;
            }

            await DisplayMessageAsync(message);

            var handler = MessageSelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public event EventHandler AccountSelectionChanged;

        private void OnAccountSelectionChanged() {
            var handler = AccountSelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public object Messages {
            get { return _messages; }
            set {
                if (_messages == value) {
                    return;
                }
                _messages = value;
                RaisePropertyChanged(() => Messages);
            }
        }

        public string QueryString {
            get { return _queryString; }
            set {
                if (_queryString == value) {
                    return;
                }
                _queryString = value;
                RaisePropertyChanged(() => QueryString);
                OnQueryStringChanged();
            }
        }

        public ReplyCommand ReplyCommand {
            get { return _replyCommand; }
        }

        public ForwardCommand ForwardCommand {
            get { return _forwardCommand; }
        }
        public DeleteCommand DeleteCommand{
            get { return _deleteCommand; }
        }

        private void OnQueryStringChanged() {

        }

        internal void UpdateMessages() {
            var mailbox = SelectedAccount.SelectedMailbox;
            Messages = mailbox.Messages;
            if (mailbox.Messages.Count > 0) {
                mailbox.Messages
                    .OrderByDescending(x => x.EntryDate)
                    .First().IsSelected = true;
            }
        }

        public IEnumerable<MailMessageContext> SelectedMessages {
            get { return _selectedMessages; }
            set {
                if (Equals(_selectedMessages, value)) {
                    return;
                }
                _selectedMessages = value;
                RaisePropertyChanged(() => SelectedMessage);
                RaisePropertyChanged(() => SelectedMessages);
                OnMessageSelectionChanged();
            }
        }

        public MailMessageContext SelectedMessage {
            get {
                return SelectedMessages == null
                    ? null
                    : SelectedMessages.FirstOrDefault();
            }
        }

        private void ClearMessageView() {
            Html = null;
        }

        private async Task DisplayMessageAsync(MailMessageContext message) {
            var mime = await message.LoadMimeFromDatabaseAsync();
            if (string.IsNullOrEmpty(mime)) {
                mime = await message.DownloadMessageAsync();
            }

            var mail = new MailMessage(Encoding.UTF8.GetBytes(mime));
            var text = mail.FindFirstHtmlVersion();
            if (text != null) {
                Html = Encoding.UTF8.GetString(text.Body);
            }
        }

        public string Html {
            get { return _html; }
            set {
                if (_html == value) {
                    return;
                }
                _html = value;
                RaisePropertyChanged(() => Html);
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
                OnAccountSelectionChanged();
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

        public async Task RunAsync() {
            await LoadAccountsAsync();
            SelectedAccount = Accounts.FirstOrDefault();
            if (SelectedAccount != null)
                SelectedAccount.IsSelected = true;
        }

        private async Task LoadAccountsAsync() {
            using (var context = new DatabaseContext()) {
                var accounts = await context.MailAccounts.ToArrayAsync();
                _accounts.AddRange(accounts.Select(x => new MailAccountContext(x)));
            }
        }
    }
}
