using Crystalbyte.Paranoia.Contexts;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Models;
using Crystalbyte.Paranoia.Properties;
using NLog;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Crystalbyte.Paranoia.Commands {

    [Export, Shared]
    [Export(typeof(IAppBarCommand))]
    public sealed class DeleteContactCommand : IAppBarCommand {

        #region Private Fields

        private ImageSource _image;

        #endregion

        #region Log Declaration

        private static Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        #region Import Declarations

        [Import]
        public IdentitySelectionSource IdentitySelectionSource { get; set; }

        [Import]
        public ContactSelectionSource ContactSelectionSource { get; set; }

        [Import]
        public AppContext AppContext { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            ContactSelectionSource.SelectionChanged +=
                (sender, e) => OnCanExecuteChanged(EventArgs.Empty);
        }

        #endregion

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return ContactSelectionSource.Contact != null;
        }

        public async void Execute(object parameter) {
            var identity = IdentitySelectionSource.Identity;
            var contact = ContactSelectionSource.Contact;

            await contact.DeleteAsync();
            identity.Contacts.Remove(contact);
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        public string Tooltip {
            get { return Resources.DeleteContactCommandToolTip; }
        }

        public string Category {
            get { return AppBarCategory.Contacts; }
        }

        public ImageSource Image {
            get {
                if (_image == null) {
                    var uri =
                        new Uri(string.Format(Pack.Application, typeof(InviteContactCommand).Assembly.FullName,
                                              "Assets/delete.png"), UriKind.Absolute);
                    _image = new BitmapImage(uri);
                }
                return _image;
            }
        }

        public int Position {
            get { return 10; }
        }
    }
}
