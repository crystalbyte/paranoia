#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Crystalbyte.Paranoia.Commands;
using Crystalbyte.Paranoia.Cryptography;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Models;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    [Export, Shared]
    public sealed class AppContext : NotificationObject {

        #region Private Fields

        private bool _isSyncing;
        private IdentityCreationContext _identityCreationContext;
        private readonly ObservableCollection<IdentityContext> _identities;
        private readonly ObservableCollection<ImapAccountContext> _imapAccounts;
        private bool _isHtmlVisible;

        #endregion

        #region Construction

        public AppContext() {
            _identities = new ObservableCollection<IdentityContext>();
            _imapAccounts = new ObservableCollection<ImapAccountContext>();

            CreateIdentityCommand = new RelayCommand(OnCreateIdentityCommandExecuted);
            AddContactCommand = new RelayCommand(OnAddContactCommandExecuted);
        }

        private void OnAddContactCommandExecuted(object obj) {
            throw new NotImplementedException();
        }

        #endregion

        #region Import Declarations

        [Import]
        public ErrorLogContext ErrorLogContext { get; set; }

        [Import]
        public DeleteContactCommand DeleteContactCommand { get; set; }

        [Import]
        public StorageContext StorageContext { get; set; }

        [Import]
        public IdentitySelectionSource IdentitySelectionSource { get; set; }

        [Import]
        public ContactSelectionSource ContactSelectionSource { get; set; }

        [Import]
        public ComposeMessageCommand ComposeMessageCommand { get; set; }

        [ImportMany]
        public IEnumerable<IAppBarCommand> AppBarCommands { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {

        }

        #endregion

        public IEnumerable<IAppBarCommand> ContactCommands {
            get {
                return AppBarCommands.Where(x => x.Category == AppBarCategory.Contacts).OrderBy(x => x.Position).ToArray();
            }
        }

        public ICommand CreateIdentityCommand { get; private set; }
        public ICommand AddContactCommand { get; private set; }

        public IdentityCreationContext IdentityCreationContext { 
            get {
                return _identityCreationContext;
            }
            set {
                if (_identityCreationContext == value) {
                    return;
                }

                RaisePropertyChanging(() => IdentityCreationContext);
                _identityCreationContext = value;
                RaisePropertyChanged(() => IdentityCreationContext);
            }
        }

        public event EventHandler SyncStatusChanged;

        public void OnSyncStatusChanged(EventArgs e) {
            var handler = SyncStatusChanged;
            if (handler != null)
                handler(this, e);
        }

        private void OnIdentityCreationFinished(object sender, EventArgs e) {
            if (IdentityCreationContext != null) {
                IdentityCreationContext.Finished -= OnIdentityCreationFinished;
            }

            IdentityCreationContext = null;
        }

        private void OnCreateIdentityCommandExecuted(object obj) {
            IdentityCreationContext = new IdentityCreationContext();
            IdentityCreationContext.Finished += OnIdentityCreationFinished;
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

        public IList<IdentityContext> Identities {
            get { return _identities; }
        }

        public void SyncAsync() {
            IsSyncing = true;

            // TODO: Implement syncing

            IsSyncing = false;
        }

        /// <summary>
        ///   The initial seed for the PRNG is fetched via random.org (http://www.random.org/clients/http/).
        /// </summary>
        /// <returns> The task state object. </returns>
        private static async Task SeedAsync() {
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
            try {
                await SeedAsync();
            } catch (Exception ex) {
                Debug.WriteLine(ex);
            }

            await StorageContext.Current.InitAsync();
            //await LoadIdentitiesAsync();
            //await LoadImapAccountsAsync();

       
        }

        //private async Task LoadImapAccountsAsync() {
        //    var query = await SelectImapAccountsAsync();
        //    ImapAccounts.AddRange(query.Select(x => new ImapAccountContext(x)));

        //    foreach (var account in ImapAccounts) {
        //        await account.LoadMailboxesAsync();
        //    }

        //    if (ImapAccounts.Any()) {
        //        ImapAccounts.First().IsSelected = true;
        //    }
        //}

        //private Task<ImapAccount[]> SelectImapAccountsAsync() {
        //    //return Task.Factory.StartNew(() => LocalStorage.Context.ImapAccounts.ToArray());
        //}

        private async Task LoadIdentitiesAsync() {
            //var query = await SelectIdentitiesAsync();
            //Identities.AddRange(query.ToArray().Select(x => new IdentityContext(x)));
            //if (Identities.Any()) {
            //    Identities.First().IsSelected = true;
            //}
        }

        //private Task<Identity[]> SelectIdentitiesAsync() {
        //    return Task.Factory.StartNew(() => StorageContext.Context.Identities.ToArray());
        //}
    }
}