#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI.Commands;
using NLog;
using System.Collections.Specialized;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class OutboxContext : HierarchyContext {
        private bool _sendingMessages;
        private bool _isLoadingCompositions;
        private readonly MailAccountContext _account;
        private readonly ObservableCollection<CompositionContext> _compositions;
        private string _queryString;
        private int _compositionCount;
        private readonly ICommand _sendMessagesCommand;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        public OutboxContext(MailAccountContext account) {
            _account = account;
            _compositions = new ObservableCollection<CompositionContext>();
            _compositions.CollectionChanged += OnSmtpRequestsCollectionChanged;

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
            await SendCompositionsAsync();
        }

        public ICommand SendMessagesCommand {
            get { return _sendMessagesCommand; }
        }

        private void OnSmtpRequestsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            RaisePropertyChanged(() => SmtpRequestCount);
            RaisePropertyChanged(() => SelectedComposition);
            RaisePropertyChanged(() => SelectedCompositions);
            App.Context.NotifySmtpRequestChanged();
        }

        internal event EventHandler<QueryStringEventArgs> QueryStringChanged;

        private void OnQueryStringChanged(QueryStringEventArgs e) {
            var handler = QueryStringChanged;
            if (handler != null)
                handler(this, e);
        }

        public IEnumerable<CompositionContext> SelectedCompositions {
            get { return _compositions.Where(x => x.IsSelected).ToArray(); }
        }

        public CompositionContext SelectedComposition {
            get { return _compositions.FirstOrDefault(x => x.IsSelected); }
        }

        private void OnQueryReceived(string obj) {
            throw new NotImplementedException();
        }

        public int SmtpRequestCount {
            get { return _compositionCount; }
            set {
                if (_compositionCount == value) {
                    return;
                }
                _compositionCount = value;
                RaisePropertyChanged(() => SmtpRequestCount);
            }
        }

        internal async Task CountSmtpRequestsAsync() {
            int count;
            using (var context = new DatabaseContext()) {
                count = await context.Compositions
                   .Where(x => x.AccountId == _account.Id)
                   .CountAsync();
            }

            await Application.Current.Dispatcher.InvokeAsync(() => {
                SmtpRequestCount = count;
            });
        }

        public event EventHandler SmtpRequestSelectionChanged;


        internal void OnCompositionSelectionChanged() {
            var handler = SmtpRequestSelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);

            RaisePropertyChanged(() => SelectedComposition);
            RaisePropertyChanged(() => SelectedCompositions);

            App.Context.NotifySmtpRequestChanged();

            var request = SelectedComposition;
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

        internal async Task LoadCompositionsAsync() {
            try {
                IsLoadingRequests = true;
                using (var database = new DatabaseContext()) {
                    var requests = await database.Compositions
                        .Where(x => x.AccountId == _account.Id)
                        .ToArrayAsync();

                    _compositions.Clear();
                    _compositions.AddRange(requests.Select(x => new CompositionContext(x)));

                    await CountSmtpRequestsAsync();
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            } finally {
                IsLoadingRequests = false;
            }
        }

        private static void ViewSmtpRequest(CompositionContext request) {
            App.Context.Source = string.Format("asset://paranoia/smtp-request/{0}", request.Id);
        }

        public bool IsLoadingRequests {
            get { return _isLoadingCompositions; }
            set {
                if (_isLoadingCompositions == value) {
                    return;
                }
                _isLoadingCompositions = value;
                RaisePropertyChanged(() => IsLoadingRequests);
            }
        }

        public void ClearSmtpRequests() {
            _compositions.Clear();
        }

        protected override void OnSelectionChanged() {
            base.OnSelectionChanged();

            RaisePropertyChanged(() => SelectedComposition);
            RaisePropertyChanged(() => SelectedCompositions);
        }

        public IEnumerable<CompositionContext> SmtpRequests {
            get { return _compositions; }
        }

        private Task<CompositionModel[]> GetPendingCompositionsAsync() {
            using (var database = new DatabaseContext()) {
                return database.Compositions
                    .Where(x => x.AccountId == _account.Id)
                    .ToArrayAsync();
            }
        }

        internal async Task SendCompositionsAsync() {
            var requests = await GetPendingCompositionsAsync();
            if (!requests.Any() || _sendingMessages) {
                return;
            }

            _sendingMessages = true;

            foreach (var request in requests) {
                try {
                    using (var connection = new SmtpConnection { Security = _account.SmtpSecurity }) {
                        using (var auth = await connection.ConnectAsync(_account.SmtpHost, _account.SmtpPort)) {

                            var username = _account.UseImapCredentialsForSmtp
                                ? _account.ImapUsername
                                : _account.SmtpUsername;

                            var password = _account.UseImapCredentialsForSmtp
                                ? _account.ImapPassword
                                : _account.SmtpPassword;

                            using (var session = await auth.LoginAsync(username, password)) {
                                var mime = Encoding.UTF8.GetString(request.Mime);
                                await session.SendAsync(mime);
                            }
                        }
                    }

                    var context = _compositions.FirstOrDefault(x => x.Id == request.Id);
                    if (context != null) {
                        _compositions.Remove(context);
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

        private static Task DeleteRequestFromDatabaseAsync(CompositionModel request) {
            using (var database = new DatabaseContext()) {
                database.Compositions.Attach(request);
                database.Compositions.Remove(request);
                return database.SaveChangesAsync();
            }
        }
    }
}