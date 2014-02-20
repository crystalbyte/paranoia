using Crystalbyte.Paranoia.Contexts;
using System;
using System.Composition;

namespace Crystalbyte.Paranoia {

    [Export, Shared]
    public sealed class MailboxSelectionSource : NotificationObject {
        private MailboxContext _mailbox;

        #region Event Declarations

        public event EventHandler SelectionChanged;

        public void OnSelectionChanged(EventArgs e) {
            var handler = SelectionChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        public MailboxContext Mailbox {
            get { return _mailbox; }
            set {
                if (_mailbox == value) {
                    return;
                }

                RaisePropertyChanging(() => Mailbox);
                _mailbox = value;
                RaisePropertyChanged(() => Mailbox);
                OnSelectionChanged(EventArgs.Empty);
            }
        }
    }
}
