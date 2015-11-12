using System;

namespace Crystalbyte.Paranoia {
    public abstract class View : SelectionObject {

        #region Private Fields
        
        private Uri _iconUri;
        private Badge _badge;
        private string _title;

        #endregion

        #region Properties

        public string Title {
            get { return _title; }
            set {
                if (_title == value) {
                    return;
                }

                _title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        public Uri PageUri {
            get {
                return GetPageUri();
            }
        }

        public Uri IconUri {
            get { return _iconUri; }
            set {
                if (_iconUri == value) {
                    return;
                }

                _iconUri = value;
                RaisePropertyChanged(() => IconUri);
            }
        }

        public Badge Badge {
            get { return _badge; }
            set {
                if (_badge == value) {
                    return;
                }

                _badge = value;
                RaisePropertyChanged(() => Badge);
            }
        }

        #endregion

        #region Methods

        public abstract Uri GetPageUri();

        #endregion
    }
}
