using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Contexts {
    [Export, Shared]
    public sealed class SettingsContext : ValidationObject<SettingsContext> {

        #region Private Fields

        private bool _isActive;

        #endregion

        #region Import Declarations

        [Import]
        public AppContext AppContext { get; set; }

        #endregion

        public bool IsActive {
            get { return _isActive; }
            set {
                if (_isActive == value) {
                    return;
                }

                RaisePropertyChanging(() => IsActive);
                _isActive = value;
                RaisePropertyChanged(() => IsActive);
            }
        }
    }
}
