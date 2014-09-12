#region Using directives

using System;
using System.Linq;
using Crystalbyte.Paranoia.Data;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class MailContactContext : SelectionObject {
        private readonly MailContactModel _contact;
        private bool _isValidated;
        private int _notSeenCount;
        private int _messageCount;

        internal MailContactContext(MailContactModel contact) {
            _contact = contact;
        }

        public bool HasUnseenMessages {
            get { return NotSeenCount > 0; }
        }

        public int NotSeenCount {
            get { return _notSeenCount; }
            set {
                if (_notSeenCount == value) {
                    return;
                }

                _notSeenCount = value;
                RaisePropertyChanged(() => NotSeenCount);
                RaisePropertyChanged(() => HasUnseenMessages);
            }
        }

        public int MessageCount {
            get { return _messageCount; }
            set {
                if (_messageCount == value) {
                    return;
                }
                _messageCount = value;
                RaisePropertyChanged(() => MessageCount);
            }
        }

        public char Group {
            get {
                var isEmpty = string.IsNullOrWhiteSpace(Name)
                    || string.Compare(Name, "nil", StringComparison.InvariantCultureIgnoreCase) == 0;
                return isEmpty ? '#' : char.ToUpper(Name.First());
            }
        }

        public bool IsValidated {
            get { return _isValidated; }
            set {
                if (_isValidated == value) {
                    return;
                }
                _isValidated = value;
                RaisePropertyChanged(() => IsValidated);
            }
        }

        public string Address {
            get { return _contact.Address; }
            set {
                if (_contact.Address == value) {
                    return;
                }

                _contact.Address = value;
                RaisePropertyChanged(() => Address);
            }
        }

        public string Name {
            get { return _contact.Name; }
            set {
                if (_contact.Name == value) {
                    return;
                }

                _contact.Name = value;
                RaisePropertyChanged(() => Name);
            }
        }
    }
}