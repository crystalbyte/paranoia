#region Using directives

using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class MailContactContext : SelectionObject {
        private readonly MailContactModel _contact;
        private bool _isVerified;
        private int _notSeenCount;
        private int _messageCount;
        private bool _hasKeys;
        private bool _isBlocked;

        internal MailContactContext(MailContactModel contact) {
            _contact = contact;
        }

        public bool HasUnseenMessages {
            get { return NotSeenCount > 0; }
        }

        public async Task NotifyKeysUpdatedAsync() {
            await CheckForKeyExistenceAsync();
            RaisePropertyChanged(() => Security);
        }

        public long Id {
            get { return _contact.Id; }
        }

        public bool IsBlocked {
            get { return _isBlocked; }
            set {
                if (_isBlocked == value) {
                    return;
                }

                _isBlocked = value;
                RaisePropertyChanged(() => IsBlocked);
            }
        }

        public SecurityMeasure Security {
            get {
                if (HasKeys && IsVerified) {
                    return SecurityMeasure.EncryptedAndVerified;
                }

                return HasKeys ? SecurityMeasure.Encrypted : SecurityMeasure.None;
            }
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

        public bool HasKeys {
            get { return _hasKeys; }
            set {
                if (_hasKeys == value) {
                    return;
                }
                _hasKeys = value;
                RaisePropertyChanged(() => HasKeys);
            }
        }

        public bool IsVerified {
            get { return _isVerified; }
            set {
                if (_isVerified == value) {
                    return;
                }
                _isVerified = value;
                RaisePropertyChanged(() => IsVerified);
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

        internal async Task CheckForKeyExistenceAsync() {
            using (var database = new DatabaseContext()) {
                HasKeys = (await database.PublicKeys
                    .Where(x => x.ContactId == _contact.Id)
                    .CountAsync()) > 0;
            }
        }
    }
}