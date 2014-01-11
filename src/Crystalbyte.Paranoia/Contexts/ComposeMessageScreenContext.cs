using System.Collections.ObjectModel;
using Crystalbyte.Paranoia.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Contexts {
    [Export, Shared]
    public sealed class ComposeMessageScreenContext : ValidationObject<ComposeMessageScreenContext> {

        #region Private Fields

        private string _subject;
        private string _content;
        private readonly ObservableCollection<ContactContext> _contacts;

        #endregion

        #region Construction

        public ComposeMessageScreenContext() {
            _contacts = new ObservableCollection<ContactContext>();                
        }

        #endregion

        [StringLength(256, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "SubjectStringLengthErrorText")]
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "SubjectRequiredErrorText")]
        public string Subject {
            get { return _subject; }
            set {
                if (_subject == value) {
                    return;
                }

                RaisePropertyChanging(() => Subject);
                _subject = value;
                RaisePropertyChanged(() => Subject);
            }
        }

        public string Content {
            get { return _content; }
            set {
                if (_content == value) {
                    return;
                }

                RaisePropertyChanging(() => Content);
                _content = value;
                RaisePropertyChanged(() => Content);
            }
        }

        public ObservableCollection<ContactContext> Contacts {
            get { return _contacts; }
        }
    }
}
