#region Using directives

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Mail;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class MailboxCandidateContext : SelectionObject {
        private bool _isLoading;
        private bool _isLoaded;
        private readonly ImapMailboxInfo _info;
        private readonly ObservableCollection<MailboxCandidateContext> _children;
        private readonly MailAccountContext _account;
        private Exception _lastException;
        private bool _isExpanded;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public MailboxCandidateContext(MailAccountContext account, ImapMailboxInfo info) {
            _info = info;
            _account = account;
            _children = new ObservableCollection<MailboxCandidateContext>();
        }

        public string Name {
            get { return _info.Name; }
        }

        public ImapMailboxInfo Info {
            get { return _info; }
        }

        public char Delimiter {
            get { return _info.Delimiter; }
        }

        public bool IsSelectable {
            get {
                return _info.Flags
                    .All(x => !x.ContainsIgnoreCase(@"\noselect"));
            }
        }

        public bool IsLoading {
            get { return _isLoading; }
            set {
                if (_isLoading == value) {
                    return;
                }

                _isLoading = value;
                RaisePropertyChanged(() => IsLoading);
            }
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

        public bool IsLoaded {
            get { return _isLoaded; }
            set {
                if (_isLoaded == value) {
                    return;
                }

                _isLoaded = value;
                RaisePropertyChanged(() => IsLoaded);
            }
        }

        public Exception LastException {
            get { return _lastException; }
            set {
                if (_lastException == value) {
                    return;
                }

                _lastException = value;
                RaisePropertyChanged(() => LastException);
            }
        }

        protected override async void OnSelectionChanged() {
            base.OnSelectionChanged();

            if (IsSelected && !IsLoaded && !IsLoading) {
                await LoadChildrenAsync();
            }
        }

        private async Task LoadChildrenAsync() {
            IsLoading = true;
            try {
                var pattern = string.Format("{0}{1}%", Name, Delimiter);
                var mailboxes = await _account.ListMailboxesAsync(pattern);

                if (mailboxes.Count > 0) {
                    _children.AddRange(mailboxes
                        .Select(x => new MailboxCandidateContext(_account, x)));
                }

                IsExpanded = true;
                IsLoaded = true;
            }
            catch (Exception ex) {
                Logger.Error(ex);
                LastException = ex;
            }
            finally {
                IsLoading = false;
            }
        }

        public object Children {
            get { return _children; }
        }
    }
}