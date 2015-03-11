using System;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.UI;
using NLog;

namespace Crystalbyte.Paranoia {
    public sealed class InsertLinkContext : NotificationObject {
        private readonly HtmlEditor _editor;

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private string _name;

        #endregion

        #region Construction

        public InsertLinkContext(HtmlEditor editor) {
            _editor = editor;
        }

        #endregion

        #region Property Declarations

        public string Name {
            get { return _name; }
            set {
                if (_name == value) {
                    return;
                }
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        #endregion

        #region Method Declarations

        public bool IsValid {
            get { return Uri.IsWellFormedUriString(Name, UriKind.Absolute); }
        }

        internal async Task CommitAsync() {
            try {
                await _editor.SetLinkAsync(Name);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        #endregion
    }
}
