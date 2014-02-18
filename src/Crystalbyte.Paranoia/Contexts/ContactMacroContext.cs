using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class ContactMacroContext : NotificationObject {

        #region Private Fields 

        private bool _isSelected;

        #endregion

        public ContactMacroContext(string text) {
            Text = text;
            IsSelected = true;
        }

        public string Text { get; private set; }

        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) {
                    return;
                }

                RaisePropertyChanging(() => IsSelected);
                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }
    }
}
