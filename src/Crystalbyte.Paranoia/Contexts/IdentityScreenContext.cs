using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Crystalbyte.Paranoia.Commands;

namespace Crystalbyte.Paranoia.Contexts {
    [Export, Shared]
    public sealed class IdentityScreenContext : NotificationObject {

        private bool _isActive;

        public IdentityScreenContext() {
            ContinueCommand = new RelayCommand(OnCanContinueExecuted, OnContinueExecuted);
            CancelCommand = new RelayCommand(OnCancelExecuted);    
        }

        public ICommand ContinueCommand { get; set; }
        public ICommand CancelCommand { get; set; }

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

        private void OnCancelExecuted(object obj) {
            IsActive = false;
        }

        private static bool OnCanContinueExecuted(object parameter) {
            return true;
        }

        private void OnContinueExecuted(object parameter) {
            throw new NotImplementedException();
        }
    }
}
