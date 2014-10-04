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
using System.Text.RegularExpressions;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class MailCompositionContext : NotificationObject {

        #region Private Fields

        private string _source;
        private string _subject;
        private readonly IEnumerable<MailAccountContext> _accounts;
        private readonly ObservableCollection<string> _recipients;
        private readonly ObservableCollection<MailContactContext> _suggestions;
        private readonly ObservableCollection<AttachmentContext> _attachments;
        private readonly ICommand _sendCommand;
        private readonly ICommand _addAttachmentCommand;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public MailCompositionContext() {
            _accounts = App.Context.Accounts;
            SelectedAccount = _accounts.FirstOrDefault();
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

        public ICommand AddAttachmentCommand {
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

        public IEnumerable<MailAccountContext> Accounts {
            get { return _accounts; }
        }

        public MailAccountContext SelectedAccount { get; set; }

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
                    .Take(20)
                    .ToArrayAsync();

                var contexts = candidates.Select(x => new MailContactContext(x)).ToArray();
                foreach (var context in contexts) {
                    await context.CheckForKeyExistenceAsync();
                }

                _suggestions.Clear();
                _suggestions.AddRange(contexts);
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
                var account = SelectedAccount;
                var messages = await CreateSmtpMessagesAsync(account);
                await account.SaveSmtpRequestsAsync(messages);
                await App.Context.NotifyOutboxNotEmpty();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }


        private MailMessage CreateMailMessage(MailAccountContext account, string recipient, string content) {
            var message = new MailMessage {
                From = new MailAddress(account.Address, account.Name)
            };

            message.To.Add(new MailAddress(recipient));
            message.IsBodyHtml = true;
            message.Subject = Subject;
            message.BodyEncoding = Encoding.UTF8;
            message.BodyTransferEncoding = TransferEncoding.Base64;
            message = HandleEmbeddedImages(message, content);

            _attachments.ForEach(x => message.Attachments.Add(new Attachment(x.FullName)));

            return message;
        }

        private static MailMessage HandleEmbeddedImages(MailMessage message, string content) {
            var body = string.Format("<html>{0}</html>", content);

            var regex = new Regex("<img (.*?)src=(.*?)>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = regex.Matches(body);

            foreach (Match x in matches) {
                var match = Regex.Match(x.Value, "src=\"(.*?)\"");
                var result = match.Value.Replace("src=", string.Empty).Trim(new[] { '"' }).Replace("asset://tempImage/", string.Empty);

                Attachment attachment;
                string name;

                var arguments = result.ToPageArguments();
                if (arguments.ContainsKey("cid") && arguments.ContainsKey("messageId")) {
                    var incommingCid = Uri.UnescapeDataString(arguments["cid"]);
                    long incommingMessageId;
                    if (!long.TryParse(arguments["messageId"], out incommingMessageId))
                        continue;

                    var bytes = ResourceInterceptor.GetAttachmentBytes(incommingCid, incommingMessageId);

                    name = "image.jpg";
                    var stream = new MemoryStream(bytes);
                    attachment = new Attachment(stream, name) { ContentId = (name + "@" + Guid.NewGuid()).Replace(" ", "") };

                } else {
                    var uri = new Uri(result, UriKind.RelativeOrAbsolute);
                    if (!uri.IsFile || !File.Exists(result))
                        continue;

                    name = new FileInfo(result).Name;
                    attachment = new Attachment(result) { ContentId = (name + "@" + Guid.NewGuid()).Replace(" ", "") };
                }

                message.Attachments.Add(attachment);
                body = body.Replace(match.Value, string.Format("src=\"cid:{0}\"", attachment.ContentId));
            }
            message.Body = body;

            return message;
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
                        continue;
                    }

                    var keys = await database.PublicKeys.Where(x => x.ContactId == contact.Id).ToArrayAsync();
                    if (keys == null || keys.Length == 0) {
                        var message = CreateMailMessage(account, recipient, e.Document);
                        messages.Add(message);
                        continue;
                    }

                    var cryptMessage = await EncryptMessageAsync(account, keys, recipient, e.Document);
                    messages.Add(cryptMessage);
                }

                return messages;
            }
        }


        private async Task<MailMessage> EncryptMessageAsync(MailAccountContext account, PublicKeyModel[] keys, string recipient, string content) {
            var message = CreateMailMessage(account, recipient, content);
            var mime = await message.ToMimeAsync();

            var bytes = Encoding.UTF8.GetBytes(mime);

            var wrapper = CreateMailMessage(account, recipient, "blubbi");
            var publicKey = Convert.ToBase64String(App.Context.KeyContainer.PublicKey);
            wrapper.Headers.Add(ParanoiaHeaderKeys.PublicKey, publicKey);

            foreach (var key in keys) {
                var keyBytes = Convert.FromBase64String(key.Data);
                var nonceBytes = PublicKeyCrypto.GenerateNonce();

                var encryptedBytes = await Task.Factory.StartNew(() =>
                App.Context.KeyContainer.EncryptWithPublicKey(bytes, keyBytes, nonceBytes));

                var writer = new BinaryWriter(new MemoryStream());
                writer.Write(nonceBytes);
                writer.Write(encryptedBytes);
                writer.Flush();

                writer.BaseStream.Seek(0, SeekOrigin.Begin);

                var view = new AlternateView(writer.BaseStream, new ContentType("application/x-setolicious"));
                wrapper.AlternateViews.Add(view);
            }

            return wrapper;
        }
    }
}