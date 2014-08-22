#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using MailMessage = System.Net.Mail.MailMessage;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class OutboxContext : SelectionObject {
        private bool _sendingMessages;
        private readonly MailAccountContext _account;
        private int _count;
        private readonly ObservableCollection<SmtpRequestContext> _smtpRequests;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public OutboxContext(MailAccountContext account) {
            _account = account;
            _smtpRequests = new ObservableCollection<SmtpRequestContext>();
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
            using (var database = new DatabaseContext()) {
                var requests = await database.SmtpRequests
                    .Where(x => x.AccountId == _account.Id)
                    .ToArrayAsync();

                _smtpRequests.Clear();
                _smtpRequests.AddRange(requests.Select(x => new SmtpRequestContext(x)));
            }
        }

        internal async Task CountMessagesAsync() {
            using (var database = new DatabaseContext()) {
                Count = await database.SmtpRequests
                    .Where(x => x.AccountId == _account.Id)
                    .CountAsync();
            }
        }

        public IEnumerable<SmtpRequestContext> SmtpRequests {
            get { return _smtpRequests; }
        }

        private static Task<string> GetHtmlCoverSheetAsync() {
            const string name = "/Resources/cover.sheet.template.html";
            var info = Application.GetResourceStream(new Uri(name, UriKind.Relative));
            if (info == null) {
                var message = string.Format(Resources.ResourceNotFoundException, name, typeof (App).Name);
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

                    using (var connection = new SmtpConnection {Security = _account.SmtpSecurity}) {
                        using (var auth = await connection.ConnectAsync(_account.SmtpHost, _account.SmtpPort)) {
                            using (var session = await auth.LoginAsync(_account.SmtpUsername, _account.SmtpPassword)) {
                                var wrapper = new MailMessage(
                                    new MailAddress(_account.Address, _account.Name),
                                    new MailAddress(request.ToAddress))
                                {
                                    Subject = string.Format(Resources.SubjectTemplate, _account.Name),
                                    Body = sheet,
                                    IsBodyHtml = true,
                                    BodyEncoding = Encoding.UTF8,
                                    HeadersEncoding = Encoding.UTF8,
                                    SubjectEncoding = Encoding.UTF8,
                                    BodyTransferEncoding = TransferEncoding.Base64,
                                };

                                var guid = Guid.NewGuid();
                                using (var writer = new StreamWriter(new MemoryStream()) {AutoFlush = true}) {
                                    await writer.WriteAsync(request.Mime);
                                    writer.BaseStream.Seek(0, SeekOrigin.Begin);
                                    wrapper.Attachments.Add(new Attachment(writer.BaseStream, guid.ToString())
                                    {
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
                }
                catch (Exception ex) {
                    _logger.Error(ex);
                }
                finally {
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
    }
}