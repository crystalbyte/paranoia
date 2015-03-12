#region Using directives

using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Crystalbyte.Paranoia.Data;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class MailContactContext : SelectionObject {

        #region Private Fields

        private bool _hasKeys;
        private bool _isVerified;
        private int _notSeenCount;
        private int _messageCount;
        private MailContactModel _contact;
        
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        internal MailContactContext(MailContactModel contact) {
            if (contact == null) {
                throw new ArgumentNullException("contact");
            }

            _contact = contact;
        }

        public bool HasUnseenMessages {
            get { return NotSeenCount > 0; }
        }

        public async Task NotifyKeysUpdatedAsync() {
            await CheckSecurityStateAsync();
            RaisePropertyChanged(() => Security);
        }

        public long Id {
            get { return _contact.Id; }
        }

        public bool IsExternalContentAllowed {
            get { return _contact.IsExternalContentAllowed; }
            set {
                if (_contact.IsExternalContentAllowed == value) {
                    return;
                }

                _contact.IsExternalContentAllowed = value;
                RaisePropertyChanged(() => IsExternalContentAllowed);
            }
        }

        public ContactClassification Classification {
            get { return _contact.Classification; }
            set {
                if (_contact.Classification == value) {
                    return;
                }

                _contact.Classification = value;
                RaisePropertyChanged(() => Classification);
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

        public string NameOrAddress {
            get { return string.IsNullOrWhiteSpace(Name) || Name.EqualsIgnoreCase("NIL") ? Address : Name; }
        }

        internal async Task CheckSecurityStateAsync() {
            Application.Current.AssertBackgroundThread();

            bool hasKeys;
            using (var database = new DatabaseContext()) {
                hasKeys = (await database.PublicKeys
                    .Where(x => x.ContactId == _contact.Id)
                    .CountAsync()) > 0;
            }

            await Application.Current.Dispatcher.InvokeAsync(() => {
                HasKeys = hasKeys;
            });
        }

        public Task SaveAsync() {
            return Task.Run(() => {
                try {
                    using (var context = new DatabaseContext()) {
                        var c = context.MailContacts.Find(Id);
                        c.Classification = _contact.Classification;

                        // Handle Optimistic Concurrency.
                        // https://msdn.microsoft.com/en-us/data/jj592904.aspx?f=255&MSPPError=-2147217396
                        while (true) {
                            try {
                                context.SaveChanges();
                                break;
                            } catch (DbUpdateConcurrencyException ex) {
                                ex.Entries.ForEach(x => x.Reload());
                                Logger.Info(ex);
                            }
                        }

                        _contact = c;
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            });
        }
    }
}