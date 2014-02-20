using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Contexts {
    [Export, Shared]
    public sealed class MailSelectionSource {
        #region Private Fields

        private ObservableCollection<MailContext> _mails;

        #endregion

        #region Construction
        public MailSelectionSource() {
            _mails = new ObservableCollection<MailContext>();
            _mails.CollectionChanged += (sender, e) => OnSelectionChanged();
        }

        #endregion

        #region Event Declarations

        public event EventHandler SelectionChanged;
        private void OnSelectionChanged() {
            var handler = SelectionChanged;
            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion

        public ObservableCollection<MailContext> Mails {
            get { return _mails; }
        }
    }
}
