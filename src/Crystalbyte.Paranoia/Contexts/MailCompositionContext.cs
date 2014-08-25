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
using System.Windows.Input;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.UI.Commands;
using NLog;
using Crystalbyte.Paranoia.Cryptography;
using Crystalbyte.Paranoia.Contexts;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class MailCompositionContext : NotificationObject {

        #region Private Fields

        private string _source;
        private string _subject;
        private readonly ObservableCollection<string> _recipients;
        private readonly ObservableCollection<MailContactContext> _suggestions;
        private readonly ObservableCollection<AttachmentContext> _attachments;
        private readonly ICommand _sendCommand;
        private readonly ICommand _addAttachmentCommand;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public MailCompositionContext() {
            _recipients = new ObservableCollection<string>();
            _suggestions = new ObservableCollection<MailContactContext>();
            _sendCommand = new SendCommand(this);
            _addAttachmentCommand = new AddAttachmentCommand(this);
            _attachments = new ObservableCollection<AttachmentContext>();
        }

        #endregion

        #region Event Declarations

        public event EventHandler Finished;

        private void OnFinished() {
            var handler = Finished;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public event EventHandler<DocumentTextRequestedEventArgs> DocumentTextRequested;

        private void OnDocumentTextRequested(DocumentTextRequestedEventArgs e) {
            var handler = DocumentTextRequested;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        #region Properties

        public ICommand SendCommand {
            get { return _sendCommand; }
        }

        public ICommand AddattachmentCommand {
            get { return _addAttachmentCommand; }
        }

        public ICollection<string> Recipients {
            get { return _recipients; }
        }

        public ICollection<AttachmentContext> Attachments {
            get { return _attachments; }
        }

        public IEnumerable<MailContactContext> Suggestions {
            get { return _suggestions; }
        }

        public string Subject {
            get { return _subject; }
            set {
                if (_subject == value) {
                    return;
                }

                _subject = value;
                RaisePropertyChanged(() => Subject);
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

        #endregion

        public async Task QueryRecipientsAsync(string text) {
            using (var database = new DatabaseContext()) {
                var candidates = await database.MailContacts
                    .Where(x => x.Address.StartsWith(text)
                                || x.Name.StartsWith(text))
                    .ToArrayAsync();

                _suggestions.Clear();
                _suggestions.AddRange(candidates.Select(x => new MailContactContext(x)));
            }
        }

        public async Task ResetAsync() {
            _recipients.Clear();
            Subject = string.Empty;

            var info = Application.GetResourceStream(new Uri("Resources/composition.template.html", UriKind.Relative));
            if (info != null) {
                using (var reader = new StreamReader(info.Stream)) {
                    Source = await reader.ReadToEndAsync();
                }
            }
        }

        internal void Finish() {
            OnFinished();
        }

        public async Task PushToOutboxAsync() {
            try {
                var account = App.Context.SelectedAccount;
                var messages = await CreateSmtpMessagesAsync(account);
                await account.SaveSmtpRequestsAsync(messages);
                await App.Context.NotifyOutboxNotEmpty();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }


        private MailMessage CreateMailMessage(MailAccountContext account, string recipient, string content) {
            return new MailMessage(
                    new MailAddress(account.Address, account.Name),
                    new MailAddress(recipient)) {
                        IsBodyHtml = true,
                        Subject = Subject,
                        Body = string.Format("<html>{0}</html>", content),
                        BodyEncoding = Encoding.UTF8,
                        BodyTransferEncoding = TransferEncoding.Base64
                    };
        }

        private async Task<IEnumerable<MailMessage>> CreateSmtpMessagesAsync(MailAccountContext account) {
            var e = new DocumentTextRequestedEventArgs();
            OnDocumentTextRequested(e);

            var messages = new List<MailMessage>();
            using (var database = new DatabaseContext()) {
                foreach (var recipient in Recipients) {
                    if (string.IsNullOrEmpty(recipient)) {
                        continue;
                    }

                    var rec = recipient;
                    var contact = await database.MailContacts.FirstOrDefaultAsync(x => x.Address == rec);
                    if (contact == null) {
                        var message = CreateMailMessage(account, recipient, e.Document);
                        messages.Add(message);
                    }

                    var keys = await database.PublicKeys.Where(x => x.ContactId == contact.Id).ToListAsync();
                    if (keys == null || keys.Count == 0) {
                        var message = CreateMailMessage(account, recipient, e.Document);
                        messages.Add(message);
                    }

                    if (keys == null)
                        continue;

                    foreach (var key in keys) {
                        var cryptMessage = await EncryptMessageAsync(account, key, recipient, e.Document);
                        messages.Add(cryptMessage);
                    }
                }

                return messages;
            }
        }


        private async Task<MailMessage> EncryptMessageAsync(MailAccountContext account, PublicKeyModel key, string recipient, string content) {
            var message = CreateMailMessage(account, recipient, content);
            var mime = await message.ToMimeAsync();

            var bytes = Encoding.UTF8.GetBytes(mime);
            var keyBytes = Convert.FromBase64String(key.Data);
            var nonceBytes = PublicKeyCrypto.GenerateNonce();

            var encryptedBytes = await Task.Factory.StartNew(() =>
                App.Context.KeyContainer.EncryptWithPublicKey(bytes, keyBytes, nonceBytes));

            var wrapper = CreateMailMessage(account, recipient, "blubbi");
            wrapper.AlternateViews.Add(new AlternateView(new MemoryStream(encryptedBytes), new ContentType("application/base64")));
            return wrapper;
        }
    }
}