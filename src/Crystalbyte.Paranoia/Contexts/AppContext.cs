using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using System.Text;

namespace Crystalbyte.Paranoia {

    [Export, Shared]
    public sealed class AppContext : NotificationObject {

        private MailAccountContext _selectedAccount;
        private IEnumerable<MailMessageContext> _selectedMessages;
        private readonly ObservableCollection<MailAccountContext> _accounts;
        private string _html;
        private object _messages;
        private string _queryString;
        private Exception _lastException;

        public AppContext() {
            _accounts = new ObservableCollection<MailAccountContext>();
        }

        public IEnumerable<MailAccountContext> Accounts {
            get { return _accounts; }
        }

        public event EventHandler MailAccountSelectionChanged;

        private void OnMailAccountSelectionChanged() {
            var handler = MailAccountSelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public object Messages {
            get { return _messages; }
            set {
                if (_messages == value) {
                    return;
                }
                _messages = value;
                RaisePropertyChanged(() => Messages);
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
                OnQueryStringChanged();
            }
        }

        private void OnQueryStringChanged() {

        }

        internal void UpdateMessages() {
            var mailbox = SelectedAccount.SelectedMailbox;
            Messages = mailbox.Messages;
        }

        public IEnumerable<MailMessageContext> SelectedMessages {
            get { return _selectedMessages; }
            set {
                if (Equals(_selectedMessages, value)) {
                    return;
                }
                _selectedMessages = value;
                RaisePropertyChanged(() => SelectedMessages);
                OnMessageSelectionChanged();
            }
        }

        private async void OnMessageSelectionChanged() {
            var message = SelectedMessages.FirstOrDefault();
            if (message == null) {
                return;
            }

            await DisplayMessageAsync(message);
        }

        private async Task DisplayMessageAsync(MailMessageContext message) {
            var mime = await message.LoadMimeFromDatabaseAsync();
            if (string.IsNullOrEmpty(mime)) {
                mime = await message.DownloadMessageAsync();
            }

            var mail = new MailMessage(Encoding.UTF8.GetBytes(mime));
            Html = Encoding.UTF8.GetString(mail.FindFirstHtmlVersion().Body);
        }

        public string Html {
            get { return _html; }
            set {
                if (_html == value) {
                    return;
                }
                _html = value;
                RaisePropertyChanged(() => Html);
            }
        }

        public MailAccountContext SelectedAccount {
            get { return _selectedAccount; }
            set {
                if (_selectedAccount == value) {
                    return;
                }

                _selectedAccount = value;
                RaisePropertyChanged(() => SelectedAccount);
                OnMailAccountSelectionChanged();
            }
        }

        public Exception LastException {
            get { return _lastException; }
            set {
                if (_lastException == value) {
                    return;
                }
                _lastException = value;
                RaisePropertyChanged(() => LastException);
            }
        }

        public async Task RunAsync() {
            await LoadAccountsAsync();
            SelectedAccount = Accounts.FirstOrDefault();
            if (SelectedAccount != null)
                SelectedAccount.IsSelected = true;
        }

        private async Task LoadAccountsAsync() {
            using (var context = new DatabaseContext()) {
                var accounts = await context.MailAccounts.ToArrayAsync();
                _accounts.AddRange(accounts.Select(x => new MailAccountContext(x)));
            }
        }
    }
}
