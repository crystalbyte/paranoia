#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using Directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Crystalbyte.Paranoia.Cryptography;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI;
using Crystalbyte.Paranoia.UI.Commands;
using Microsoft.Win32;
using NLog;
using MailMessage = System.Net.Mail.MailMessage;

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
        private bool _isFinalizing;
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

        public bool IsFinalizing {
            get { return _isFinalizing; }
            set {
                if (_isFinalizing == value) {
                    return;
                }
                _isFinalizing = value;
                RaisePropertyChanged(() => IsFinalizing);
                OnFinalizingChanged();
            }
        }

        private void OnFinalizingChanged() {
            _finalizeCommand.OnCanExecuteChanged();
        }

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
            IsFinalizing = true;

            OnCompositionFinalizing();
            await SaveToOutboxAsync();

            IsFinalizing = false;
            OnCompositionFinalized();
        }

        public async Task SaveToOutboxAsync() {
            try {
                var account = SelectedAccount;
                var document = _provider.GetDocumentAsync();

                KeyPair current = null;

                var contacts = await Task.Run(() => {
                    using (var context = new DatabaseContext()) {
                        current = context.KeyPairs.OrderByDescending(x => x.Date).First();
                        return Addresses
                            .Select(x => context.MailContacts
                                .Include("Keys")
                                .FirstOrDefault(y => y.Address == x))
                            .Where(w => w != null)
                            .ToDictionary(z => z.Address);
                    }
                });

                var messages = new List<MailMessage>();
                foreach (var address in Addresses) {
                    PublicKey key = null;
                    if (contacts.ContainsKey(address)) {
                        var contact = contacts[address];
                        key = contact.Keys.FirstOrDefault();
                    }

                    var message = new MailMessage {
                        IsBodyHtml = true,
                        Subject = Subject,
                        BodyEncoding = Encoding.UTF8,
                        BodyTransferEncoding = TransferEncoding.Base64,
                        From = new MailAddress(account.Address, account.Name),
                        Body = await document
                    };

                    var signet = string.Format("pkey={0};", Convert.ToBase64String(current.PublicKey));
                    message.Headers.Add(MessageHeaders.Signet, signet);

                    message.To.Add(new MailAddress(address));
                    foreach (var a in Attachments) {
                        message.Attachments.Add(new Attachment(a.FullName));
                    }

                    // IO heavy operation, needs to run in background thread.
                    var m = message;
                    await Task.Run(() => m.PackageEmbeddedContent());

                    if (key == null) {
                        messages.Add(message);
                    } else {
                        var nonce = PublicKeyCrypto.GenerateNonce();
                        var payload = await EncryptMessageAsync(message, current, key, nonce);
                        message = GenerateDeliveryMessage(account, current.PublicKey, address, nonce);
                        message.AlternateViews.Add(new AlternateView(new MemoryStream(payload),
                            new ContentType(MediaTypes.EncryptedMime)));
                        messages.Add(message);
                    }
                }

                await account.SaveCompositionsAsync(messages);
                await App.Context.NotifyOutboxNotEmpty();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private static MailMessage GenerateDeliveryMessage(MailAccountContext account, byte[] pKey, string address,
            byte[] nonce) {
            var message = new MailMessage {
                Subject = "BAZINGA !!",
                BodyEncoding = Encoding.UTF8,
                BodyTransferEncoding = TransferEncoding.Base64,
                From = new MailAddress(account.Address, account.Name),
            };

            message.To.Add(address);

            const string path = "/Resources/message.facade.html";
            var info = Application.GetResourceStream(new Uri(path, UriKind.Relative));
            if (info == null) {
                throw new ResourceNotFoundException(path);
            }

            var html = AlternateView.CreateAlternateViewFromString(info.Stream.ToUtf8String());
            html.ContentType = new ContentType("text/html");
            message.AlternateViews.Add(html);

            var signet = string.Format("pkey={0};", Convert.ToBase64String(pKey));
            message.Headers.Add(MessageHeaders.Signet, signet);
            message.Headers.Add(MessageHeaders.Nonce, Convert.ToBase64String(nonce));
            return message;
        }

        private static async Task<byte[]> EncryptMessageAsync(MailMessage message, KeyPair pair,
            PublicKey pKey, byte[] nonce) {
            var mime = await message.ToMimeAsync();
            var bytes = Encoding.UTF8.GetBytes(mime);
            var crypto = new PublicKeyCrypto(pair.PublicKey, pair.PrivateKey);
            return crypto.EncryptWithPublicKey(bytes, pKey.Data, nonce);
        }

        private async void OnFinalize(object obj) {
            try {
                await FinalizeAsync();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private bool OnCanFinalize(object obj) {
            return Addresses.Any() && !IsFinalizing;
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
                    Filter = string.Format("{0} (*.*)|*.*", Resources.AllFiles)
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
                var result = match.Value.Replace("src=", string.Empty)
                    .Trim(new[] { '"' })
                    .Replace("asset://tempImage/", string.Empty);

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