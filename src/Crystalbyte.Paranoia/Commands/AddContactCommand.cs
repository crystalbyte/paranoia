using System.Net.Cache;
using Crystalbyte.Paranoia.Contexts;
using Crystalbyte.Paranoia.Properties;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Crystalbyte.Paranoia.Commands {
    [Export, Shared]
    [Export(typeof(IAppBarCommand))]
    public sealed class AddContactCommand : IAppBarCommand {

        #region Private Fields

        private BitmapImage _image;

        #endregion

        #region Import Declarations

        [Import]
        public AppContext AppContext { get; set; }

        #endregion

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            AppContext.AddContactScreenContext.IsActive = true;
        }

        public event EventHandler CanExecuteChanged;
        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        #region Implementation of IAppBarCommand

        public string Tooltip {
            get { return Resources.AddContactCommandTooltip; }
        }

        public ImageSource Image {
            get {
                if (_image == null) {
                    var uri =
                        new Uri(string.Format(Pack.Application, typeof(AddContactCommand).Assembly.FullName,
                                              "Assets/add.png"), UriKind.Absolute);
                    _image = new BitmapImage(uri);
                }
                return _image;
            }
        }

        public int Position { 
            get { return 5; }
        }

        public string Category {
            get { return AppBarCategory.Contacts; }
        }

        #endregion
    }
}
