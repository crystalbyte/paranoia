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
using System.Windows.Input;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI;
using Crystalbyte.Paranoia.UI.Commands;
using System.Windows.Media.Animation;
using Crystalbyte.Paranoia.Contexts;
using Crystalbyte.Paranoia.UI.Pages;

#endregion

namespace Crystalbyte.Paranoia {
    [Export, Shared]
    public sealed class AppContext : NotificationObject {

        #region Private Fields

        private MailAccountContext _selectedAccount;
        private IEnumerable<MailMessageContext> _selectedMessages;
        private readonly ObservableCollection<MailAccountContext> _accounts;
        private readonly PrintCommand _printCommand;
        private readonly ReplyCommand _replyCommand;
        private readonly CloseOverlayCommand _closeOverlayCommand;
        private readonly DeleteMessageCommand _deleteCommand;
        private readonly ComposeMessageCommand _writeCommand;
        private readonly ForwardCommand _forwardCommand;
        private Storyboard _overlaySlideInStoryboard;
        private FocusSearchBoxCommand _focusSearchBoxCommand;
        private string _queryString;
        private object _messages;
        private string _html;
        private bool _isOverlayed;

        #endregion

        #region Construction

        public AppContext() {
            _accounts = new ObservableCollection<MailAccountContext>();
            _replyCommand = new ReplyCommand(this);
            _forwardCommand = new ForwardCommand(this);
            _deleteCommand = new DeleteMessageCommand(this);
            _printCommand = new PrintCommand(this);
            _writeCommand = new ComposeMessageCommand(this);
            _closeOverlayCommand = new CloseOverlayCommand(this);

            Observable.FromEventPattern(
                    action => MessageSelectionChanged += action,
                    action => MessageSelectionChanged -= action)
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Subscribe(OnMessageSelectionCommittedAsync);

            Observable.FromEventPattern<QueryStringEventArgs>(
                    action => QueryStringChanged += action,
                    action => QueryStringChanged -= action)
                .Select(x => x.EventArgs)
                .Where(x => (x.Text.Length > 2 || string.IsNullOrEmpty(x.Text))
                            && string.Compare(x.Text, Resources.SearchBoxWatermark,
                                StringComparison.InvariantCultureIgnoreCase) != 0)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Select(x => x.Text)
                .Subscribe(OnQueryReceived);
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

        internal event EventHandler OverlayChanged;

        private void OnOverlayChanged() {
            var handler = OverlayChanged;
            if (handler != null) 
                handler(this, EventArgs.Empty);
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

        public bool IsOverlayed {
            get { return _isOverlayed; }
            set {
                if (_isOverlayed == value) {
                    return;
                }

                _isOverlayed = value;
                RaisePropertyChanged(() => IsNotOverlayed);
                RaisePropertyChanged(() => IsOverlayed);
                OnOverlayChanged();
                if (value) {
                    _overlaySlideInStoryboard.Begin();
                }
            }
        }

        public bool IsNotOverlayed { 
            get { return !IsOverlayed; } }

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

        public ICommand CloseOverlayCommand { 
            get { return _closeOverlayCommand; }
        }

        public ICommand FocusSearchBoxCommand {
            get { return _focusSearchBoxCommand; }
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

        public void ComposeMessage() {
            var uri = typeof(ComposeMessagePage).ToPageUri();
            OnNavigationRequested(new NavigationRequestedEventArgs(uri));
            IsOverlayed = true;
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

        internal void HookUpSearchBox(Control control) {
            _focusSearchBoxCommand = new FocusSearchBoxCommand(this, control);
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

        private void OnMessageSelectionCommittedAsync(EventPattern<object> obj) {
            //Debug.WriteLine("Message selected.");
            ClearMessageView();
            var message = SelectedMessages.FirstOrDefault();
            if (message == null) {
                return;
            }

            DisplayMessageAsync(message);
            MarkMessagesAsSeenAsync();
        }

        internal void RegisterOverlaySlideInStoryboard(Storyboard storyboard) {
            _overlaySlideInStoryboard = storyboard;
        }

        private async void MarkMessagesAsSeenAsync() {
            var mailbox = SelectedAccount.SelectedMailbox;
            await mailbox.MarkAsSeenAsync(SelectedMessages.ToArray());
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
    }
}