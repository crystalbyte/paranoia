#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Commands;
using Crystalbyte.Paranoia.Contexts.Factories;
using Crystalbyte.Paranoia.Cryptography;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Models;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    [Export, Shared]
    public sealed class AppContext : NotificationObject {
        private bool _isSyncing;
        private string _messageBody;

        public AppContext() {
            ImapAccounts = new ObservableCollection<ImapAccountContext>();
            SmtpAccounts = new ObservableCollection<SmtpAccountContext>();
        }

        public event EventHandler SyncStatusChanged;

        public void OnSyncStatusChanged(EventArgs e) {
            var handler = SyncStatusChanged;
            if (handler != null)
                handler(this, e);
        }

        [Import]
        public DeleteMessageCommand DeleteMessageCommand { get; set; }

        [Import]
        public ImapMessageSelectionSource ImapMessageSelectionSource { get; set; }

        [Import]
        public ImapAccountContextFactory ImapAccountContextFactory { get; set; }

        [Import]
        public SmtpAccountContextFactory SmtpAccountContextFactory { get; set; }

        [Import]
        public ComposeMessageCommand ComposeMessageCommand { get; set; }

        [Import]
        public LocalStorage LocalStorage { get; set; }

        [Import]
        public SyncCommand SyncCommand { get; set; }

        public ObservableCollection<ImapAccountContext> ImapAccounts { get; private set; }
        public ObservableCollection<SmtpAccountContext> SmtpAccounts { get; private set; }

        public string MessageBody {
            get { return _messageBody; }
            set {
                if (_messageBody == value) {
                    return;
                }

                RaisePropertyChanging(() => MessageBody);
                _messageBody = value;
                RaisePropertyChanged(() => MessageBody);
            }
        }

        public bool IsSyncing {
            get { return _isSyncing; }
            set {
                if (_isSyncing == value) {
                    return;
                }

                RaisePropertyChanging(() => IsSyncing);
                _isSyncing = value;
                RaisePropertyChanged(() => IsSyncing);
                OnSyncStatusChanged(EventArgs.Empty);
            }
        }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            //var imap = new ImapAccount
            //               {
            //                   Host = "imap.gmail.com",
            //                   Port = 993,
            //                   Username = "paranoia.app@gmail.com",
            //                   Password = "p4r4n014"
            //               };

            //var account = ImapAccountContextFactory.Create(imap);
            //ImapAccounts.Add(account);

            //var smtp = new SmtpAccount
            //               {
            //                   Host = "smtp.gmail.com",
            //                   Port = 587,
            //                   Username = "paranoia.app@gmail.com",
            //                   Password = "p4r4n014"
            //               };

            //SmtpAccounts.Add(SmtpAccountContextFactory.Create(smtp));
        }

        public IEnumerable<ImapMessageContext> Messages {
            get { return ImapAccounts.SelectMany(x => x.Messages); }
        }
        
        public async Task SyncAsync() {
            IsSyncing = true;

            ImapAccounts.ForEach(x => x.Messages.Clear());

            foreach (var account in ImapAccounts) {
                await account.SyncAsync();
            }

            RaisePropertyChanged(() => Messages);
            IsSyncing = false;
        }

        /// <summary>
        ///   The initial seed for the PRNG is fetched via random.org (http://www.random.org/clients/http/).
        /// </summary>
        /// <returns> The task state object. </returns>
        public async Task SeedAsync() {
            var url =
                string.Format(
                    "http://www.random.org/integers/?num={0}&min=0&max=999999999&col=1&base=10&format=plain&rnd=new",
                    1024);

            using (var client = new WebClient()) {
                var stream = await client.OpenReadTaskAsync(url);
                using (var reader = new StreamReader(stream)) {
                    var text = await reader.ReadToEndAsync();
                    var bytes = Encoding.UTF8.GetBytes(text.Replace("\n", string.Empty));
                    OpenSslRandom.Seed(bytes, bytes.Length);
                }
            }
        }

        public async Task RunAsync() {
            await SeedAsync();
            await LocalStorage.InitAsync();
        }
    }
}