﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.UI.Commands;
using System.Net.Mail;
using MailMessage = System.Net.Mail.MailMessage;

namespace Crystalbyte.Paranoia {
    public sealed class MailCompositionContext : NotificationObject {

        #region Private Fields

        private string _text;
        private string _subject;
        private readonly ObservableCollection<string> _recipients;
        private readonly ObservableCollection<MailContactContext> _suggestions;
        private readonly ICommand _sendCommand;

        #endregion

        #region Construction

        public MailCompositionContext() {
            _recipients = new ObservableCollection<string>();
            _suggestions = new ObservableCollection<MailContactContext>();
            _sendCommand = new SendCommand(this);
        }

        #endregion

        #region Event Declarations

        public event EventHandler ShutdownRequested;

        private void OnShutdownRequested() {
            var handler = ShutdownRequested;
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

        public ICollection<string> Recipients {
            get { return _recipients; }
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

        public string Text {
            get { return _text; }
            set {
                if (_text == value) {
                    return;
                }
                _text = value;
                RaisePropertyChanged(() => Text);
            }
        }

        #endregion

        public async Task QueryRecipientsAsync(string text) {
            var account = App.Context.SelectedAccount;

            using (var database = new DatabaseContext()) {
                var candidates = await database.MailContacts
                    .Where(x => x.AccountId == account.Id)
                    .Where(x => x.Address.StartsWith(text) || x.Name.StartsWith(text))
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
                    Text = await reader.ReadToEndAsync();
                }
            }
        }

        public async Task PushToOutboxAsync() {
            try {
                var account = App.Context.SelectedAccount;
                var messages = CreateSmtpMessages(account);
                await account.SaveSmtpRequestsAsync(messages);
                await App.Context.NotifyOutboxNotEmpty();
            }
            catch (Exception) {
                throw;
            }
            finally {
                OnShutdownRequested();
            }
        }

        private IEnumerable<MailMessage> CreateSmtpMessages(MailAccountContext account) {
            var e = new DocumentTextRequestedEventArgs();
            OnDocumentTextRequested(e);

            return (from recipient in Recipients
                    select new MailMessage(
                        new MailAddress(account.Address, account.Name),
                        new MailAddress(recipient)) {
                            IsBodyHtml = true,
                            Subject = Subject,
                            Body = string.Format("<html>{0}</html>", e.Document),
                            BodyEncoding = Encoding.UTF8,
                            BodyTransferEncoding = TransferEncoding.Base64
                        }).ToList();
        }
    }
}
