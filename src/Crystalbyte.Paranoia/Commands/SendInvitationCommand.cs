using System.Diagnostics;
using System.Net.Mail;
using System.Net.Mime;
using Crystalbyte.Paranoia.Messaging;
using System;
using System.Composition;
using System.Threading.Tasks;
using System.Windows.Input;
using Crystalbyte.Paranoia.Messaging.Mime;
using Crystalbyte.Paranoia.Properties;
using System.Windows;
using System.IO;
using System.Text;
using NLog;
using Crystalbyte.Paranoia.Contexts;

namespace Crystalbyte.Paranoia.Commands {
    [Export, Shared]
    public sealed class SendInvitationCommand : ICommand {

        #region Import Declarations

        [Import]
        public IdentitySelectionSource IdentitySelectionSource { get; set; }

        [Import]
        public ContactSelectionSource ContactSelectionSource { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            IdentitySelectionSource.SelectionChanged += (sender, e) => OnCanExecuteChanged(EventArgs.Empty);
            ContactSelectionSource.SelectionChanged += (sender, e) => OnCanExecuteChanged(EventArgs.Empty);
        }

        #endregion

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return IdentitySelectionSource.Identity != null 
                && ContactSelectionSource.Contact != null;
        }

        public async void Execute(object parameter) {
            await ContactSelectionSource.Contact.SendInviteAsync();
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        #region Log Declaration

        private static Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

      
    }
}
