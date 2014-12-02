#region Using directives

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.UI.Commands;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class CreateContactContext : NotificationObject {
        private readonly ICommand _createContactCommand;
        private string _name;
        private string _address;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CreateContactContext() {
            _createContactCommand = new RelayCommand(OnCreateContact);
        }

        private async void OnCreateContact(object obj) {
            try {
                var contact = await SaveContactToDatabaseAsync();
                App.Context.NotifyContactsAdded(new[] { new MailContactContext(contact) });
            } catch (Exception ex) {
                Logger.Error(ex);
            } finally {
                App.Context.ClosePopup();
            }
        }

        private async Task<MailContactModel> SaveContactToDatabaseAsync() {
            var contact = new MailContactModel {
                Name = Name,
                Address = Address
            };

            using (var database = new DatabaseContext()) {
                database.MailContacts.Add(contact);
                await database.SaveChangesAsync();
            }
            return contact;
        }

        public ICommand CreateContactCommand {
            get { return _createContactCommand; }
        }

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

        public string Address {
            get { return _address; }
            set {
                if (_address == value) {
                    return;
                }

                _address = value;
                RaisePropertyChanged(() => Address);
            }
        }
    }
}