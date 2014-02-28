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
using Crystalbyte.Paranoia.Cryptography;
using Crystalbyte.Paranoia.Data;
using NLog;
using System.Windows.Input;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    [Export, Shared]
    public sealed class AppContext : NotificationObject {

        #region Private Fields

        private bool _isSyncing;
        private readonly ObservableCollection<IdentityContext> _identities;

        #endregion

        #region Log Declaration

        private static Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public AppContext() {
            _identities = new ObservableCollection<IdentityContext>();
        }

        #endregion

        #region Event Declarations

        public event EventHandler SyncStatusChanged;
        public void OnSyncStatusChanged(EventArgs e) {
            var handler = SyncStatusChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        #region Import Declarations

        [Import]
        public DeleteIdentityCommand DeleteIdentityCommand { get; set; }

        [Import]
        public DeleteContactCommand DeleteContactCommand { get; set; }

        [Import]
        public IdentitySelectionSource IdentitySelectionSource { get; set; }

        [Import]
        public ContactSelectionSource ContactSelectionSource { get; set; }

        [Import]
        public MailboxSelectionSource MailboxSelectionSource { get; set; }

        [Import]
        public MailSelectionSource MailSelectionSource { get; set; }

        [Import]
        public ComposeMessageCommand ComposeMessageCommand { get; set; }

        [Import]
        public IdentityCreationContext IdentityCreationContext { get; set; }

        [Import]
        public ContactInvitationContext ContactInvitationContext { get; set; }

        [Import]
        public InviteContactCommand InviteContactCommand { get; set; }

        [Import]
        public CreateIdentityCommand CreateIdentityCommand { get; set; }

        [ImportMany]
        public IEnumerable<IAppBarCommand> AppBarCommands { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            IdentityCreationContext.Finished += OnIdentityCreationFinished;
            ContactInvitationContext.Finished += OnContactInvitationContextFinished;
        }

        #endregion

        public IEnumerable<IAppBarCommand> ContactCommands {
            get {
                return AppBarCommands
                    .Where(x => x.Category == AppBarCategory.Contacts)
                    .OrderBy(x => x.Position).ToArray();
            }
        }

        public IEnumerable<IAppBarCommand> MailCommands {
            get {
                return AppBarCommands
                    .Where(x => x.Category == AppBarCategory.Mails)
                    .OrderBy(x => x.Position).ToArray();
            }
        }

        private async void OnIdentityCreationFinished(object sender, EventArgs e) {
            await RestoreIdentitiesAsync();
        }

        private async void OnContactInvitationContextFinished(object sender, EventArgs e) {
            var identity = IdentitySelectionSource.Identity;
            if (identity != null) {
                await identity.RestoreContactsAsync();
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

            Log.Info("Starting application ...");

            try {
                await SeedAsync();
            } catch (Exception ex) {
                Log.Error(ex.Message);
            }

            using (var context = new StorageContext()) {
                await context.InitAsync();
                await RestoreIdentitiesAsync(context);
            }
        }

        internal async Task RestoreIdentitiesAsync(StorageContext context = null) {
            IEnumerable<IdentityContext> idents = null;
            await Task.Factory.StartNew(() => {
                using (var c = context ?? new StorageContext()) {
                    idents = c.Identities.ToArray()
                        .Select(x => new IdentityContext(x)).ToArray();
                }
            });

            Identities.Clear();
            Identities.AddRange(idents);
            if (Identities.Any()) {
                Identities.First().IsSelected = true;
            }
        }

        internal void ShowInvitationScreen() {
            ContactInvitationContext.IsActive = true;
        }

        internal void ShowIdentityCreationScreen() {
            IdentityCreationContext.IsActive = true;
        }
    }
}