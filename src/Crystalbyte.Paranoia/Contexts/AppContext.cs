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

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    [Export, Shared]
    public sealed class AppContext : NotificationObject {
        #region Private Fields

        private bool _isSyncing;
        private readonly ObservableCollection<object> _elements;
        private readonly ObservableCollection<IdentityContext> _identities;
        private readonly ObservableCollection<object> _accounts;

        #endregion

        #region Construction

        public AppContext() {
            _elements = new ObservableCollection<object>();
            _identities = new ObservableCollection<IdentityContext>();
            _accounts = new ObservableCollection<object>();

            CreateAccountCommand = new RelayCommand(OnCreateAccountCommandExecuted);
            CreateIdentityCommand = new RelayCommand(OnCreateIdentityCommandExecuted);
            OpenSettingsCommand = new RelayCommand(OnOpenSettingsCommandExecuted);
        }

        #endregion

        #region Import Declarations

        [Import]
        public LocalStorage LocalStorage { get; set; }

        [Import]
        public CreateIdentityScreenContext CreateIdentityScreenContext { get; set; }

        [Import]
        public CreateAccountScreenContext CreateAccountScreenContext { get; set; }

        [Import]
        public SettingsContext SettingsContext { get; set; }

        [Import]
        public IdentitySelectionSource IdentitySelectionSource { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
      
        }

        #endregion

        public ICommand CreateAccountCommand { get; private set; }
        public ICommand CreateIdentityCommand { get; private set; }
        public ICommand OpenSettingsCommand { get; private set; }

        public event EventHandler SyncStatusChanged;

        public void OnSyncStatusChanged(EventArgs e) {
            var handler = SyncStatusChanged;
            if (handler != null)
                handler(this, e);
        }

        private void OnCreateAccountCommandExecuted(object obj) {
            CreateAccountScreenContext.IsActive = true;
        }

        private void OnCreateIdentityCommandExecuted(object obj) {
            CreateIdentityScreenContext.IsActive = true;
        }

        private void OnOpenSettingsCommandExecuted(object obj) {
            SettingsContext.IsActive = true;
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

        public IList<object> Accounts {
            get { return _accounts; }
        }

        public IList<IdentityContext> Identities {
            get { return _identities; }
        }

        public IList<object> Elements {
            get { return _elements; }
        }

        public void SyncAsync() {
            IsSyncing = true;

            // TODO: Implement syncing

            RaisePropertyChanged(() => Elements);
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
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
            }

            await LocalStorage.InitAsync();
            await LoadIdentities();
        }

        private async Task LoadIdentities() {
            var query = await LocalStorage.QueryIdentitiesAsync();
            Identities.AddRange(query.ToArray().Select(x => new IdentityContext(x)));
            if (Identities.Any()) {
                Identities.First().IsSelected = true;
            }
        }
    }
}