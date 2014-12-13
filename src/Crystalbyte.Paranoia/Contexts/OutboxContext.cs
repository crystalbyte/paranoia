#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI;
using Crystalbyte.Paranoia.UI.Commands;
using NLog;
using System.Collections.Specialized;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class OutboxContext : HierarchyContext, IKeyboardFocusAware {
        private bool _sendingMessages;
        private bool _isLoadingRequests;
        private readonly MailAccountContext _account;
        private readonly ObservableCollection<SmtpRequestContext> _smtpRequests;
        private string _queryString;
        private int _smtpRequestCount;
        private bool _isKeyboardFocused;
        private readonly ICommand _sendMessagesCommand;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        

        public OutboxContext(MailAccountContext account) {
            _account = account;
            _smtpRequests = new ObservableCollection<SmtpRequestContext>();
            _smtpRequests.CollectionChanged += OnSmtpRequestsCollectionChanged;

            _sendMessagesCommand = new RelayCommand(OnSendMessages);

            Observable.FromEventPattern<QueryStringEventArgs>(
                action => QueryStringChanged += action,
                action => QueryStringChanged -= action)
                    .Select(x => x.EventArgs)
                    .Where(x => (x.Text.Length > 1 || string.IsNullOrEmpty(x.Text)))
                    .Throttle(TimeSpan.FromMilliseconds(200))
                    .Select(x => x.Text)
                    .ObserveOn(new DispatcherSynchronizationContext(Application.Current.Dispatcher))
                    .Subscribe(OnQueryReceived);
        }

        private async void OnSendMessages(object obj) {
            await ProcessOutgoingMessagesAsync();
        }

        public ICommand SendMessagesCommand {
            get { return _sendMessagesCommand; }
        }

        private void OnSmtpRequestsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            RaisePropertyChanged(() => SmtpRequestCount);
            RaisePropertyChanged(() => SelectedSmtpRequest);
            RaisePropertyChanged(() => SelectedSmtpRequests);
            App.Context.NotifySmtpRequestChanged();
        }

        internal event EventHandler<QueryStringEventArgs> QueryStringChanged;

        private void OnQueryStringChanged(QueryStringEventArgs e) {
            var handler = QueryStringChanged;
            if (handler != null)
                handler(this, e);
        }

        public IEnumerable<SmtpRequestContext> SelectedSmtpRequests {
            get { return _smtpRequests.Where(x => x.IsSelected).ToArray(); }
        }

        public SmtpRequestContext SelectedSmtpRequest {
            get { return _smtpRequests.FirstOrDefault(x => x.IsSelected); }
        }

        private void OnQueryReceived(string obj) {
            throw new NotImplementedException();
        }

        public int SmtpRequestCount {
            get { return _smtpRequestCount; }
            set {
                if (_smtpRequestCount == value) {
                    return;
                }
                _smtpRequestCount = value;
                RaisePropertyChanged(() => SmtpRequestCount);
            }
        }

        internal async Task CountSmtpRequestsAsync() {
            using (var context = new DatabaseContext()) {
                SmtpRequestCount = await context.SmtpRequests
                    .Where(x => x.AccountId == _account.Id)
                    .CountAsync();
            }
        }

        public event EventHandler SmtpRequestSelectionChanged;


        internal void OnSmtpRequestSelectionChanged() {
            var handler = SmtpRequestSelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);

            RaisePropertyChanged(() => SelectedSmtpRequest);
            RaisePropertyChanged(() => SelectedSmtpRequests);

            App.Context.NotifySmtpRequestChanged();

            var request = SelectedSmtpRequest;
            if (request != null) {
                ViewSmtpRequest(request);
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
                OnQueryStringChanged(new QueryStringEventArgs(value));
            }
        }

        public string Name {
            get { return Resources.Outbox; }
        }

        internal async Task LoadSmtpRequestsAsync() {
            try {
                IsLoadingRequests = true;
                using (var database = new DatabaseContext()) {
                    var requests = await database.SmtpRequests
                        .Where(x => x.AccountId == _account.Id)
                        .ToArrayAsync();

                    _smtpRequests.Clear();
                    _smtpRequests.AddRange(requests.Select(x => new SmtpRequestContext(x)));

                    await CountSmtpRequestsAsync();
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            } finally {
                IsLoadingRequests = false;
            }
        }

        private static void ViewSmtpRequest(SmtpRequestContext request) {
            App.Context.Source = string.Format("asset://paranoia/smtp-request/{0}", request.Id);
        }

        public bool IsLoadingRequests {
            get { return _isLoadingRequests; }
            set {
                if (_isLoadingRequests == value) {
                    return;
                }
                _isLoadingRequests = value;
                RaisePropertyChanged(() => IsLoadingRequests);
            }
        }

        public void ClearSmtpRequests() {
            _smtpRequests.Clear();
        }

        protected override void OnSelectionChanged() {
            base.OnSelectionChanged();

            RaisePropertyChanged(() => SelectedSmtpRequest);
            RaisePropertyChanged(() => SelectedSmtpRequests);
        }

        public IEnumerable<SmtpRequestContext> SmtpRequests {
            get { return _smtpRequests; }
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

        private Task<SmtpRequestModel[]> GetPendingSmtpRequestsAsync() {
            using (var database = new DatabaseContext()) {
                return database.SmtpRequests
                    .Where(x => x.AccountId == _account.Id)
                    .ToArrayAsync();
            }
        }

        internal async Task ProcessOutgoingMessagesAsync() {
            var requests = await GetPendingSmtpRequestsAsync();
            if (!requests.Any() || _sendingMessages) {
                return;
            }

            _sendingMessages = true;

            foreach (var request in requests) {
                try {
                    using (var connection = new SmtpConnection { Security = _account.SmtpSecurity }) {
                        using (var auth = await connection.ConnectAsync(_account.SmtpHost, _account.SmtpPort)) {
                            using (var session = await auth.LoginAsync(_account.SmtpUsername, _account.SmtpPassword)) {
                                await session.SendAsync(request.Mime);
                            }
                        }
                    }

                    var context = _smtpRequests.FirstOrDefault(x => x.Id == request.Id);
                    if (context != null) {
                        _smtpRequests.Remove(context);
                    }

                    await DeleteRequestFromDatabaseAsync(request);
                    await CountSmtpRequestsAsync();
                } catch (Exception ex) {
                    Logger.Error(ex);
                } finally {
                    _sendingMessages = false;
                }
            }
        }

        private static Task DeleteRequestFromDatabaseAsync(SmtpRequestModel request) {
            using (var database = new DatabaseContext()) {
                database.SmtpRequests.Attach(request);
                database.SmtpRequests.Remove(request);
                return database.SaveChangesAsync();
            }
        }

        #region Implementation of IKeyboardFocusAware

        public bool IsKeyboardFocused {
            get { return _isKeyboardFocused; }
            set {
                if (_isKeyboardFocused == value) {
                    return;
                }
                _isKeyboardFocused = value;
                RaisePropertyChanged(() => IsKeyboardFocused);
            }
        }

        #endregion
    }
}