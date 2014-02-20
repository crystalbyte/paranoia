#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Crystalbyte.Paranoia.Models;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;
using NLog;
using System.Data.Entity;
using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class IdentityContext : NotificationObject {

        #region Private Fields

        private readonly Identity _identity;
        private readonly ObservableCollection<object> _contacts;
        private readonly SmtpAccountContext _smtpAccount;
        private readonly ImapAccountContext _imapAccount;
        private string _gravatarImageUrl;
        private bool _isSelected;

        #endregion

        #region Log Declaration

        private static Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public IdentityContext()
            : this(new Identity()) { }

        public IdentityContext(Identity identity) {
            _identity = identity;
            _contacts = new ObservableCollection<object>();
            _imapAccount = new ImapAccountContext(_identity.ImapAccount);
            _smtpAccount = new SmtpAccountContext(_identity.SmtpAccount);
        }

        #endregion

        public ObservableCollection<object> Contacts {
            get { return _contacts; }
        }

        public ImapAccountContext ImapAccount { 
            get { return _imapAccount; }
        }

        public IEnumerable<MailboxContext> Mailboxes {
            get { return _imapAccount.Mailboxes; } 
        }

        public SmtpAccountContext SmtpAccount {
            get { return _smtpAccount; }
        }

        public int Id {
            get { return _identity.Id; }
        }

        public string Name {
            get { return _identity.Name; }
            set {
                if (_identity.Name == value) {
                    return;
                }

                RaisePropertyChanging(() => Name);
                _identity.Name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public string Address {
            get { return _identity.Address; }
            set {
                if (_identity.Address == value) {
                    return;
                }

                RaisePropertyChanging(() => Address);
                _identity.Address = value;
                RaisePropertyChanged(() => Address);
                CreateGravatarUrl();
            }
        }

        private void CreateGravatarUrl() {
            _gravatarImageUrl = Gravatar.CreateImageUrl(Address);
        }

        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) {
                    return;
                }

                RaisePropertyChanging(() => IsSelected);
                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);
                OnSelected(EventArgs.Empty);
            }
        }

        public event EventHandler Selected;

        private async void OnSelected(EventArgs e) {
            var handler = Selected;
            if (handler != null) {
                handler(this, e);
            }

            await RestoreContactsAsync();
            await SyncMailboxesAsync();
        }

        private async Task SyncMailboxesAsync() {
            await ImapAccount.SyncMailboxesAsync();
            RaisePropertyChanged(() => Mailboxes);

            var inbox = Mailboxes.FirstOrDefault(x => x.IsInbox);
            if (inbox != null) {
                inbox.IsSelected = true;
            }
        }

        internal async Task RestoreContactsAsync() {
            IEnumerable<ContactContext> contacts = null;
            await Task.Factory.StartNew(() => {
                try {
                    using (var context = new StorageContext()) {
                        var id = context.Identities.First(x => x.Id == _identity.Id);
                        contacts = id.Contacts.Select(x => new ContactContext(x)).ToArray();
                    }
                } catch (Exception ex) {
                    Log.Error(ex.Message);
                }
            });

            _contacts.Clear();
            _contacts.Add(new ContactMacroContext(Resources.AllContactsMacroText));
            _contacts.AddRange(contacts);
        }

        public string GravatarUrl {
            get {
                if (string.IsNullOrWhiteSpace(_gravatarImageUrl)) {
                    CreateGravatarUrl();
                }
                return _gravatarImageUrl;
            }
            set {
                if (_gravatarImageUrl == value) {
                    return;
                }

                RaisePropertyChanging(() => GravatarUrl);
                _gravatarImageUrl = value;
                RaisePropertyChanged(() => GravatarUrl);
            }
        }

        internal async Task DeleteAsync() {
            await Task.Factory.StartNew(() => {
                try {
                    using (var context = new StorageContext()) {
                        var id = context.Identities.First(x => x.Id == _identity.Id);
                        context.Identities.Remove(id);
                        context.SaveChanges();
                    }
                } catch (Exception ex) {
                    Log.Error(ex.Message);
                }
            });
        }

        internal async Task<ContactContext> AddContactAsync(Contact contact) {
            await Task.Factory.StartNew(() => {
                try {
                    using (var context = new StorageContext()) {
                        var id = context.Identities.First(x => x.Id == _identity.Id);
                        id.Contacts.Add(contact);
                        context.SaveChanges();
                    }
                } catch (Exception ex) {
                    Log.Error(ex.Message);
                }
            });

            return new ContactContext(contact);
        }
    }
}