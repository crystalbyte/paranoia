namespace Crystalbyte.Paranoia {
    public abstract class HierarchyContext : SelectionObject {
        private bool _isExpanded;

        public virtual bool IsListed {
            get { return true; }
        }

        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                if (_isExpanded == value) {
                    return;
                }

                _isExpanded = value;
                RaisePropertyChanged(() => IsExpanded);
            }
        }
    }
}
