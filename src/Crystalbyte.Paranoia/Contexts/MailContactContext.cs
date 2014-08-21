﻿#region Using directives

using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;

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

        internal async Task CountMessagesAsync() {
            var account = App.Context.SelectedAccount;
            if (account == null) {
                NotSeenCount = 0;
                return;
            }

            using (var context = new DatabaseContext()) {
                MessageCount = await context.MailMessages
                    .Where(x => x.Type == MailType.Message)
                    .Where(x => x.FromAddress == Address)
                    .Where(x => x.Mailbox.AccountId == account.Id)
                    .CountAsync();
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

        internal async Task CountNotSeenAsync() {
            var account = App.Context.SelectedAccount;
            if (account == null) {
                NotSeenCount = 0;
                return;
            }

            using (var context = new DatabaseContext()) {
                NotSeenCount = await context.MailMessages
                    .Where(x => x.Type == MailType.Message)
                    .Where(x => x.FromAddress == Address)
                    .Where(x => x.Mailbox.AccountId == account.Id)
                    .Where(x => !x.Flags.Contains(MailboxFlags.Seen))
                    .CountAsync();
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