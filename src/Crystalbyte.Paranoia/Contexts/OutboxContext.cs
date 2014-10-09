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
using System.Windows.Threading;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using NLog;
using System.Collections.Specialized;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class OutboxContext : HierarchyContext {
        private int _count;
        private string _source;
        private bool _sendingMessages;
        private bool _isLoadingRequests;
        private readonly MailAccountContext _account;
        private readonly ObservableCollection<SmtpRequestContext> _smtpRequests;
        private string _queryString;       

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public OutboxContext(MailAccountContext account) {
            _account = account;
            _smtpRequests = new ObservableCollection<SmtpRequestContext>();
            _smtpRequests.CollectionChanged += OnSmtpRequestsCollectionChanged;

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

        private void OnSmtpRequestsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            RaisePropertyChanged(() => SmtpRequestCount);
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
            get { return _smtpRequests.Count; }
        }

        public event EventHandler SmtpRequestSelectionChanged;
        

        internal void OnSmtpRequestSelectionChanged() {
            var handler = SmtpRequestSelectionChanged;
            if (handler != null) 
                handler(this, EventArgs.Empty);

            RaisePropertyChanged(() => SelectedSmtpRequest);
            RaisePropertyChanged(() => SelectedSmtpRequests);

            var request = SelectedSmtpRequest;
            if (request != null) {
                PreviewSmtpRequest(request);
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

        public int Count {
            get { return _count; }
            set {
                if (_count == value) {
                    return;
                }
                _count = value;
                RaisePropertyChanged(() => Count);
            }
        }

        public void Clear() {
            _smtpRequests.Clear();
        }

        internal async Task LoadSmtpRequestsFromDatabaseAsync() {
            try {
                IsLoadingRequests = true;
                using (var database = new DatabaseContext()) {
                    var requests = await database.SmtpRequests
                        .Where(x => x.AccountId == _account.Id)
                        .ToArrayAsync();

                    _smtpRequests.Clear();
                    _smtpRequests.AddRange(requests.Select(x => new SmtpRequestContext(x)));
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            } finally {
                IsLoadingRequests = false;
            }
        }

        public string Source {
            get { return _source; }
            set {
                if (_source == value) {
                    return;
                }
                _source = value;
                RaisePropertyChanged(() => Source);
            }
        }

        private void PreviewSmtpRequest(SmtpRequestContext request) {
            Source = string.Format("asset://paranoia/smtp-request/{0}", request.Id);
        }

        internal async Task CountMessagesAsync() {
            using (var database = new DatabaseContext()) {
                Count = await database.SmtpRequests
                    .Where(x => x.AccountId == _account.Id)
                    .CountAsync();
            }
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

            //var sheet = await GetHtmlCoverSheetAsync();
            //sheet = sheet.Replace("%FROM%", _account.Name);

            foreach (var request in requests) {
                try {
                    using (var connection = new SmtpConnection { Security = _account.SmtpSecurity }) {
                        using (var auth = await connection.ConnectAsync(_account.SmtpHost, _account.SmtpPort)) {
                            using (var session = await auth.LoginAsync(_account.SmtpUsername, _account.SmtpPassword)) {
                                //var wrapper = new MailMessage(
                                //    new MailAddress(_account.Address, _account.Name),
                                //    new MailAddress(request.ToAddress))
                                //{
                                //    Subject = string.Format(Resources.SubjectTemplate, _account.Name),
                                //    Body = sheet,
                                //    IsBodyHtml = true,
                                //    BodyEncoding = Encoding.UTF8,
                                //    HeadersEncoding = Encoding.UTF8,
                                //    SubjectEncoding = Encoding.UTF8,
                                //    BodyTransferEncoding = TransferEncoding.Base64,
                                //};

                                //var guid = Guid.NewGuid();
                                //using (var writer = new StreamWriter(new MemoryStream()) {AutoFlush = true}) {
                                //    await writer.WriteAsync(request.Mime);
                                //    writer.BaseStream.Seek(0, SeekOrigin.Begin);
                                //    wrapper.Attachments.Add(new Attachment(writer.BaseStream, guid.ToString())
                                //    {
                                //        TransferEncoding = TransferEncoding.Base64,
                                //        NameEncoding = Encoding.UTF8
                                //    });

                                await session.SendAsync(request.Mime);
                                //}
                            }
                        }
                    }

                    // TODO: foreach public key => end }

                    await DeleteRequestFromDatabaseAsync(request);
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
    }
}