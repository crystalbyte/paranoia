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
        private readonly ObservableCollection<MessageContext> _collection;

        public ImapMessageSelectionSource() {
            _collection = new ObservableCollection<MessageContext>();
        }

        public event EventHandler CollectionChanged;

        public void OnCollectionChanged(EventArgs e) {
            var handler = CollectionChanged;
            if (handler != null)
                handler(this, e);
        }

        public void ChangeSelection(IEnumerable<MessageContext> envelopes) {
            _collection.Clear();
            _collection.AddRange(envelopes);
            OnCollectionChanged(EventArgs.Empty);
        }

        public IEnumerable<MessageContext> SelectedMessages {
            get { return _collection; }
        }
    }
}