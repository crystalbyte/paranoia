#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Crystalbyte.Paranoia.UI.Commands;
using NLog;
using System.Text.RegularExpressions;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class MailCompositionContext : NotificationObject {

        #region Private Fields

        private string _source;
        private string _subject;
        private readonly IEnumerable<MailAccountContext> _accounts;
        private readonly ObservableCollection<string> _recipients;
        private readonly ObservableCollection<AttachmentContext> _attachments;
        private readonly ICommand _sendCommand;
        private readonly ICommand _addAttachmentCommand;
        private MailAccountContext _selectedAccount;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public MailCompositionContext() {
            _accounts = App.Context.Accounts;
            _selectedAccount = _accounts.FirstOrDefault();
            _recipients = new ObservableCollection<string>();
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

        public IEnumerable<MailAccountContext> Accounts {
            get { return _accounts; }
        }

        public MailAccountContext SelectedAccount {
            get { return _selectedAccount; }
            set {
                if (_selectedAccount == value) {
                    return;
                }
                _selectedAccount = value;
                RaisePropertyChanged(() => SelectedAccount);
            }
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

        internal async Task SendAsync() {
            OnFinished();
            //OnSmtpRequestCommitted();
            await PushToOutboxAsync();
        }

        public async Task PushToOutboxAsync() {
            try {
                var account = SelectedAccount;
                var messages = CreateSmtpMessages(account);
                await account.SaveSmtpRequestsAsync(messages);
                await App.Context.NotifyOutboxNotEmpty();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private MailMessage CreateMailMessage(MailAccountContext account, string address, string content) {
            var message = new MailMessage {
                From = new MailAddress(account.Address, account.Name)
            };

            message.To.Add(new MailAddress(address));
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

                    //var bytes = ResourceInterceptor.GetAttachmentBytes(incommingCid, incommingMessageId);

                    //name = "image.jpg";
                    //var stream = new MemoryStream(bytes);
                    //attachment = new Attachment(stream, name) { ContentId = (name + "@" + Guid.NewGuid()).Replace(" ", "") };

                } else {
                    var uri = new Uri(result, UriKind.RelativeOrAbsolute);
                    if (!uri.IsFile || !File.Exists(result))
                        continue;

                    name = new FileInfo(result).Name;
                    attachment = new Attachment(result) { ContentId = (name + "@" + Guid.NewGuid()).Replace(" ", "") };
                }

                //message.Attachments.Add(attachment);
                //body = body.Replace(match.Value, string.Format("src=\"cid:{0}\"", attachment.ContentId));
            }
            message.Body = body;

            return message;
        }

        private IEnumerable<MailMessage> CreateSmtpMessages(MailAccountContext account) {
            var e = new DocumentTextRequestedEventArgs();
            OnDocumentTextRequested(e);
            return (from recipient in Recipients
                    where !string.IsNullOrEmpty(recipient)
                    select CreateMailMessage(account, recipient, e.Document)).ToList();
        }
    }
}