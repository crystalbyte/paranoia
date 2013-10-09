#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using Crystalbyte.Paranoia.Contexts;

#endregion

namespace Crystalbyte.Paranoia {
    [Export, Shared]
    public sealed class ImapMessageSelectionSource {
        private readonly ObservableCollection<ImapMessageContext> _collection;

        public ImapMessageSelectionSource() {
            _collection = new ObservableCollection<ImapMessageContext>();
        }

        public event EventHandler CollectionChanged;

        public void OnCollectionChanged(EventArgs e) {
            var handler = CollectionChanged;
            if (handler != null)
                handler(this, e);
        }

        public void ChangeSelection(IEnumerable<ImapMessageContext> envelopes) {
            _collection.Clear();
            _collection.AddRange(envelopes);
            OnCollectionChanged(EventArgs.Empty);
        }

        public IEnumerable<ImapMessageContext> SelectedMessages {
            get { return _collection; }
        }
    }
}