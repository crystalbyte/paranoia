#region Using directives

using System;

#endregion

namespace Crystalbyte.Paranoia {
    public abstract class SelectionObject : NotificationObject {
        private bool _isSelected;

        public event EventHandler SelectionChanged;

        protected virtual void OnSelectionChanged() {
            var handler = SelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) {
                    return;
                }

                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);
                OnSelectionChanged();
            }
        }
    }
}