using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Crystalbyte.Paranoia {
    public sealed class DeferredObservableCollection<T> : ObservableCollection<T> {

        public bool DeferNotifications { get; set; }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            if (DeferNotifications) {
                return;
            }

            base.OnCollectionChanged(e);
        }

        internal void NotifyCollectionChanged() {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
