using System;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class ConfigTestContext : NotificationObject {

        #region Private Fields

        private TestResult _result;
        private string _text;
        private Exception _error;

        #endregion
        public TestResult Result {
            get { return _result; }
            set {
                if (_result == value) {
                    return;
                }

                RaisePropertyChanging(() => Result);
                RaisePropertyChanging(() => IsSuccessful);
                _result = value;
                RaisePropertyChanged(() => Result);
                RaisePropertyChanged(() => IsSuccessful);
            }
        }
        public bool IsSuccessful {
            get { return Result == TestResult.Success; }
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
                    Result = TestResult.Failure;
                }
            }
        }
    }
}
