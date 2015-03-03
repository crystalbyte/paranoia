#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Crystalbyte.Paranoia.UI;
using Crystalbyte.Paranoia.UI.Commands;
using Microsoft.Win32;
using NLog;
using System.Text.RegularExpressions;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class MailCompositionContext : NotificationObject {

        #region Private Fields

        private string _subject;
        private readonly IDocumentProvider _provider;
        private readonly IEnumerable<MailAccountContext> _accounts;
        private readonly ObservableCollection<string> _addresses;
        private readonly ObservableCollection<FileAttachmentContext> _attachments;
        private readonly RelayCommand _finalizeCommand;
        private readonly ICommand _insertAttachmentCommand;
        private MailAccountContext _selectedAccount;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public MailCompositionContext(IDocumentProvider provider) {
            _provider = provider;
            _accounts = App.Context.Accounts;
            _selectedAccount = _accounts.FirstOrDefault();
            _addresses = new ObservableCollection<string>();
            _addresses.CollectionChanged += OnAddressesCollectionChanged;
            _finalizeCommand = new RelayCommand(OnCanFinalize, OnFinalize);
            _insertAttachmentCommand = new RelayCommand(OnInsertAttachment);
            _attachments = new ObservableCollection<FileAttachmentContext>();
        }

        private void OnAddressesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            _finalizeCommand.OnCanExecuteChanged();
        }

        #endregion

        #region Event Declarations

        public event EventHandler CompositionFinalizing;

        private void OnCompositionFinalizing() {
            var handler = CompositionFinalizing;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler CompositionFinalized;

        private void OnCompositionFinalized() {
            var handler = CompositionFinalized;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region Properties

        public ICommand FinalizeCommand {
            get { return _finalizeCommand; }
        }

        public ICommand AddAttachmentCommand {
            get { return _insertAttachmentCommand; }
        }

        public ICollection<string> Addresses {
            get { return _addresses; }
        }

        public ICollection<FileAttachmentContext> Attachments {
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
                OnSubjectChanged();
            }
        }

        private void OnSubjectChanged() {
            _finalizeCommand.OnCanExecuteChanged();
        }

        #endregion

        internal async Task FinalizeAsync() {
            OnCompositionFinalizing();
            await SaveToOutboxAsync();
            OnCompositionFinalized();
        }

        public async Task SaveToOutboxAsync() {
            try {
                var account = SelectedAccount;
                var document = await _provider.GetDocumentAsync();

                var messages = new List<MailMessage>();
                foreach (var address in Addresses) {
                    var message = new MailMessage {
                        IsBodyHtml = true,
                        Subject = Subject,
                        BodyEncoding = Encoding.UTF8,
                        BodyTransferEncoding = TransferEncoding.Base64,
                        From = new MailAddress(account.Address, account.Name),
                        Body = document,
                    };

                    message.To.Add(new MailAddress(address));
                    foreach (var a in Attachments) {
                        message.Attachments.Add(new Attachment(a.FullName));
                    }

                    // TODO: Embedded images need to be redone after CefSharp change.
                    // message = HandleEmbeddedImages(message, content);

                    messages.Add(message);
                }

                await account.SaveCompositionsAsync(messages);
                await App.Context.NotifyOutboxNotEmpty();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private async void OnFinalize(object obj) {
            try {
                await FinalizeAsync();
            } catch (Exception ex) {
                Logger.Error(ex);
            }

        }

        private bool OnCanFinalize(object obj) {
            return Addresses.Any();
        }

        private void OnInsertAttachment(object obj) {
            try {
                InsertAttachments();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        internal void InsertAttachments() {
            try {
                var dialog = new OpenFileDialog {
                    Multiselect = true,
                    Filter = string.Format("{0} (*.*)|*.*", Properties.Resources.AllFiles)
                };

                // Display OpenFileDialog by calling ShowDialog method 
                var result = dialog.ShowDialog();
                if (!(result.HasValue && result.Value)) {
                    return;
                }

                _attachments.AddRange(dialog.FileNames
                    .Select(name => new FileInfo(name))
                    .Where(x => x.Exists)
                    .Select(x => new FileAttachmentContext(x.FullName)));
            } catch (Exception ex) {
                Logger.Error(ex);
            }
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


    }
}