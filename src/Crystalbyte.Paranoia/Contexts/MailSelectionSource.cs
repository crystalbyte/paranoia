using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Contexts {
    [Export, Shared]
    public sealed class MailSelectionSource : NotificationObject {

        #region Private Fields

        private MailContext _current;

        #endregion

        #region Construction

        public MailSelectionSource() {
            Mails = new ObservableCollection<MailContext>();
            Mails.CollectionChanged += (sender, e) => OnSelectionChanged();
        }

        #endregion

        #region Event Declarations

        public event EventHandler SelectionChanged;
        private void OnSelectionChanged() {
            Current = Mails.FirstOrDefault();
            var handler = SelectionChanged;
            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion

        public MailContext Current {
            get { return _current; }
            set {
                if (_current == value) {
                    return;
                }

                RaisePropertyChanging(() => Current);
                _current = value;
                RaisePropertyChanged(() => Current);
            }
        }

        public ObservableCollection<MailContext> Mails { get; private set; }
    }
}
