#region Using directives

using System;
using System.Composition;
using System.Globalization;
using System.Linq;
using Crystalbyte.Paranoia.Contexts;
using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Properties;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using NLog;
using Crystalbyte.Paranoia.Data;
using System.Collections.Generic;

#endregion

namespace Crystalbyte.Paranoia.Commands {
    [Export, Shared]
    [Export(typeof(IAppBarCommand))]
    public sealed class DeleteMailsCommand : IAppBarCommand {

        private ImageSource _image;

        #region Log Declaration

        private static Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        #region Import Declarations

        [Import]
        public AppContext AppContext { get; set; }

        [Import]
        public MailboxSelectionSource MailboxSelectionSource { get; set; }

        [Import]
        public MailSelectionSource MailSelectionSource { get; set; }

        [Import]
        public IdentitySelectionSource IdentitySelectionSource { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            MailSelectionSource.SelectionChanged += (sender, e) => OnCanExecuteChanged(EventArgs.Empty);
        }

        #endregion

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return MailSelectionSource.Mails.Count > 0;
        }

        public async void Execute(object parameter) {
            // Make a copy, because we can't remove items from and enumerate a collection at the same time.
            var mails = MailSelectionSource.Mails.ToArray();
            var mailbox = MailboxSelectionSource.Mailbox;
            var identity = IdentitySelectionSource.Identity;
            var account = identity.ImapAccount;

            foreach (var mail in mails) {
                mailbox.Mails.Remove(mail);
            }

            using (var connection = new ImapConnection { Security = account.Security }) {
                using (var authenticator = await connection.ConnectAsync(account.Host, account.Port)) {
                    using (var session = await authenticator.LoginAsync(account.Username, account.Password)) {
                        var box = await session.SelectAsync(mailbox.Name);
                        try {
                            await DeleteCachedMailsAsync(mails);
                            await box.DeleteMailsAsync(mails.Select(x => x.Uid));
                        } catch (Exception ex) {
                            Log.Error(ex.Message);
                        }
                    }
                }
            }
        }

        private static async Task DeleteCachedMailsAsync(IEnumerable<MailContext> mails) {
            try {
                var ids = mails.Select(x => x.Id.ToString(CultureInfo.InvariantCulture)).Aggregate((c,n) => c + ", " + n);
                var command = string.Format("DELETE FROM Mails WHERE Id IN ({0})",ids) ;
                using (var context = new StorageContext()) {
                    await context.Database.ExecuteSqlCommandAsync(command);
                }
            } catch (Exception ex) {
                Log.Error(ex.Message);
            }
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        public string Tooltip {
            get { return Resources.DeleteMailCommandTooltip; }
        }

        public string Category {
            get { return AppBarCategory.Mails; }
        }

        public ImageSource Image {
            get {
                if (_image != null) return _image;
                var uri =
                    new Uri(string.Format(Pack.Application, typeof(InviteContactCommand).Assembly.FullName,
                        "Assets/delete.png"), UriKind.Absolute);
                _image = new BitmapImage(uri);
                return _image;
            }
        }

        public int Position {
            get { return 10; }
        }
    }
}