using System;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class SettingsTestContext : NotificationObject {

        #region Private Fields

        private bool _isActive;
        private bool _isSuccessful;
        private string _text;
        private Exception _error;

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

        public bool IsSuccessful {
            get { return _isSuccessful; }
            set {
                if (_isSuccessful == value) {
                    return;
                }

                RaisePropertyChanging(() => IsSuccessful);
                _isSuccessful = value;
                RaisePropertyChanged(() => IsSuccessful);
            }
        }

        public string Text {
            get { return _text; }
            set {
                if (_text == value) {
                    return;
                }

                RaisePropertyChanging(() => Text);
                _text = value;
                RaisePropertyChanged(() => Text);
            }
        }

        public Exception Error {
            get { return _error; }
            set {
                if (_error == value) {
                    return;
                }

                RaisePropertyChanging(() => Error);
                _error = value;
                RaisePropertyChanged(() => Error);

                if (_error != null) {
                    IsSuccessful = false;
                }
            }
        }
    }
}
