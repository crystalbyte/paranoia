#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Data.Entity;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI;
using Crystalbyte.Paranoia.UI.Commands;
using Crystalbyte.Paranoia.Contexts;
using Crystalbyte.Paranoia.UI.Pages;

#endregion

namespace Crystalbyte.Paranoia {
    [Export, Shared]
    public sealed class AppContext : NotificationObject {

        #region Private Fields

        private readonly DispatcherTimer _outboxTimer;
        private MailAccountContext _selectedAccount;
        private IEnumerable<MailMessageContext> _selectedMessages;
        private readonly ObservableCollection<MailAccountContext> _accounts;
        private readonly ICommand _printCommand;
        private readonly ICommand _replyCommand;
        private readonly ICommand _deleteCommand;
        private readonly ICommand _writeCommand;
        private readonly ICommand _forwardCommand;
        private readonly ICommand _markAsSeenCommand;
        private readonly ICommand _markAsNotSeenCommand;
        private string _queryString;
        private object _messages;
        private string _html;

        #endregion

        #region Construction

        public AppContext() {
            _accounts = new ObservableCollection<MailAccountContext>();
            _printCommand = new PrintCommand(this);
            _replyCommand = new ReplyCommand(this);
            _forwardCommand = new ForwardCommand(this);
            _deleteCommand = new DeleteMessageCommand(this);
            _writeCommand = new ComposeMessageCommand(this);
            _markAsSeenCommand = new MarkAsSeenCommand(this);
            _markAsNotSeenCommand = new MarkAsNotSeenCommand(this);

            Observable.FromEventPattern(
                    action => MessageSelectionChanged += action,
                    action => MessageSelectionChanged -= action)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .Subscribe(OnMessageSelectionCommitted);

            Observable.FromEventPattern<QueryStringEventArgs>(
                    action => QueryStringChanged += action,
                    action => QueryStringChanged -= action)
                .Select(x => x.EventArgs)
                .Where(x => (x.Text.Length > 2 || string.IsNullOrEmpty(x.Text))
                            && string.Compare(x.Text, Resources.SearchBoxWatermark,
                                StringComparison.InvariantCultureIgnoreCase) != 0)
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Select(x => x.Text)
                .Subscribe(OnQueryReceived);

            _outboxTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _outboxTimer.Tick += OnOutboxTimerTick;
        }

        #endregion

        #region Public Events

        internal event EventHandler<NavigationRequestedEventArgs> NavigationRequested;

        private void OnNavigationRequested(NavigationRequestedEventArgs e) {
            var handler = NavigationRequested;
            if (handler != null) {
                handler(this, e);
            }
        }

        internal event EventHandler MessageSelectionChanged;
        private void OnMessageSelectionChanged() {
            var handler = MessageSelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
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

        #endregion

        #region Property Declarations

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

        public IEnumerable<MailAccountContext> Accounts {
            get { return _accounts; }
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

        public ICommand PrintCommand {
            get { return _printCommand; }
        }

        public ICommand WriteMessageCommand {
            get { return _writeCommand; }
        }

        public ICommand ReplyCommand {
            get { return _replyCommand; }
        }

        public ICommand ForwardCommand {
            get { return _forwardCommand; }
        }

        public ICommand DeleteMessageCommand {
            get { return _deleteCommand; }
        }

        public ICommand MarkAsSeenCommand {
            get { return _markAsSeenCommand; }
        }

        public ICommand MarkAsNotSeenCommand {
            get { return _markAsNotSeenCommand; }
        }

        internal void ComposeMessage() {
            var uri = typeof(ComposeMessagePage).ToPageUri();
            OnNavigationRequested(new NavigationRequestedEventArgs(uri));
        }

        internal void ComposeReplyMessage()
        {
            if (SelectedMessage == null)
            {
                return;
            }
            var uri = typeof(ComposeMessagePage).ToPageUriReply(SelectedMessage);
            OnNavigationRequested(new NavigationRequestedEventArgs(uri));
        }

        internal void CloseOverlay() {
            var uri = typeof(BlankPage).ToPageUri();
            OnNavigationRequested(new NavigationRequestedEventArgs(uri));
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

        #endregion

        private async void OnQueryReceived(string text) {
            var mailbox = SelectedAccount.SelectedMailbox;
            if (string.IsNullOrEmpty(text)) {
                DisplayMessages(mailbox.Messages);
                return;
            }

            var messages = await mailbox.QueryAsync(text);
            DisplayMessages(messages);
        }

        private async void OnMessageSelectionCommitted(EventPattern<object> obj) {
            ClearMessageView();
            var message = SelectedMessages.FirstOrDefault();
            if (message == null) {
                return;
            }

            DisplayMessageAsync(message);
            await MarkSelectionAsSeenAsync();
        }

        internal async Task MarkSelectionAsSeenAsync() {
            var tasks = SelectedMessages
                .Where(x => x.IsNotSeen)
                .GroupBy(x => x.Mailbox)
                .Select(x => x.Key.MarkAsSeenAsync(x.ToArray()));

            await Task.WhenAll(tasks);
        }

        internal async Task MarkSelectionAsNotSeenAsync() {
            var tasks = SelectedMessages
                .Where(x => x.IsSeen)
                .GroupBy(x => x.Mailbox)
                .Select(x => x.Key.MarkAsNotSeenAsync(x.ToArray()));

            await Task.WhenAll(tasks);
        }

        internal void DisplayMessages(ICollection<MailMessageContext> messages) {
            Messages = messages;
            if (messages == null) {
                return;
            }

            if (messages.Count > 0) {
                messages.OrderByDescending(x => x.EntryDate)
                    .First().IsSelected = true;
            }
        }

        private void ClearMessageView() {
            Html = null;
        }

        private async void DisplayMessageAsync(MailMessageContext message) {
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

        public async Task RunAsync() {
            await LoadAccountsAsync();
            SelectedAccount = Accounts.FirstOrDefault();
            if (SelectedAccount != null)
                SelectedAccount.IsSelected = true;

            _outboxTimer.Start();
        }

        private async Task LoadAccountsAsync() {
            using (var context = new DatabaseContext()) {
                var accounts = await context.MailAccounts.ToArrayAsync();
                _accounts.AddRange(accounts.Select(x => new MailAccountContext(x, this)));
            }
        }

        internal void ClearMessages() {
            Messages = null;
            ClearMessageView();
        }

        public void NotifyMessageCountChanged() {
            RaisePropertyChanged(() => Messages);
        }

        private async void OnOutboxTimerTick(object sender, EventArgs e) {
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return;
            }

            await ProcessOutgoingMessagesAsync();
        }

        private Task ProcessOutgoingMessagesAsync() {
            var tasks = Accounts.Select(x => x.ProcessOutgoingMessagesAsync());
            return Task.WhenAll(tasks);
        }

        internal async Task NotifyOutboxNotEmpty() {
            await ProcessOutgoingMessagesAsync();
        }
    }
}