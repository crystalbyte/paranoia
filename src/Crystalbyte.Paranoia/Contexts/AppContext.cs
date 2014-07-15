#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI.Commands;

#endregion

namespace Crystalbyte.Paranoia {
    [Export, Shared]
    public sealed class AppContext : NotificationObject {
        private MailAccountContext _selectedAccount;
        private IEnumerable<MailMessageContext> _selectedMessages;
        private readonly ObservableCollection<MailAccountContext> _accounts;
        private readonly ReplyCommand _replyCommand;
        private readonly DeleteCommand _deleteCommand;
        private readonly ForwardCommand _forwardCommand;
        private FocusSearchBoxCommand _focusSearchBoxCommand;
        private Exception _lastException;
        private string _queryString;
        private object _messages;
        private string _html;

        public AppContext() {
            _accounts = new ObservableCollection<MailAccountContext>();
            _replyCommand = new ReplyCommand(this);
            _forwardCommand = new ForwardCommand(this);
            _deleteCommand = new DeleteCommand(this);

            var queryStringObservable = Observable
                .FromEventPattern<QueryStringEventArgs>(
                    action => QueryStringChanged += action,
                    action => QueryStringChanged -= action)
                .Select(x => x.EventArgs);

            queryStringObservable
                .Where(x => (x.Text.Length > 2 || string.IsNullOrEmpty(x.Text))
                            && string.Compare(x.Text, Resources.SearchBoxWatermark,
                                StringComparison.InvariantCultureIgnoreCase) != 0)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Select(x => x.Text)
                .Subscribe(OnQueryReceived);
        }

        internal void HookUpSearchBox(Control control) {
            _focusSearchBoxCommand = new FocusSearchBoxCommand(control);
            RaisePropertyChanged(() => FocusSearchBoxCommand);
        }

        private async void OnQueryReceived(string text) {
            var mailbox = SelectedAccount.SelectedMailbox;
            if (string.IsNullOrEmpty(text)) {
                DisplayMessages(mailbox.Messages);
                return;
            }

            using (var context = new DatabaseContext()) {
                var messages = await context.MailMessages
                    .Where(x => x.Subject.Contains(text) && x.MailboxId == mailbox.Id)
                    .ToArrayAsync();
                var contexts = messages.Select(x => new MailMessageContext(x));
                DisplayMessages(contexts.ToArray());
            }
        }

        public IEnumerable<MailAccountContext> Accounts {
            get { return _accounts; }
        }

        internal event EventHandler MessageSelectionChanged;

        private async void OnMessageSelectionChanged() {
            await HandleMessageSelectionChangedAsync();
            var handler = MessageSelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private async Task HandleMessageSelectionChangedAsync() {
            ClearMessageView();
            var message = SelectedMessages.FirstOrDefault();
            if (message == null) {
                return;
            }

            await DisplayMessageAsync(message);
        }

        internal event EventHandler AccountSelectionChanged;

        private void OnAccountSelectionChanged() {
            var handler = AccountSelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        internal event EventHandler<QueryStringEventArgs> QueryStringChanged;

        private void OnQueryStringChanged(QueryStringEventArgs e) {
            var handler = QueryStringChanged;
            if (handler != null)
                handler(this, e);
        }

        public string QueryString {
            get { return _queryString; }
            set {
                if (_queryString == value) {
                    return;
                }
                _queryString = value;
                RaisePropertyChanged(() => QueryString);
                OnQueryStringChanged(new QueryStringEventArgs(value));
            }
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

        public FocusSearchBoxCommand FocusSearchBoxCommand {
            get { return _focusSearchBoxCommand; }
        }

        public ReplyCommand ReplyCommand {
            get { return _replyCommand; }
        }

        public ForwardCommand ForwardCommand {
            get { return _forwardCommand; }
        }

        public DeleteCommand DeleteCommand {
            get { return _deleteCommand; }
        }

        internal void DisplayMessages(ICollection<MailMessageContext> messages) {
            Messages = messages;
            if (messages.Count > 0) {
                messages.OrderByDescending(x => x.EntryDate)
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