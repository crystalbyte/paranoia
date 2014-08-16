using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.UI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Crystalbyte.Paranoia {
    public sealed class CreateContactContext : NotificationObject {

        private readonly ICommand _createContactCommand;
        private string _name;
        private string _address;


        public CreateContactContext() {
            _createContactCommand = new RelayCommand(OnCreateContact);
        }

        private async void OnCreateContact(object obj) {
            var contact = await SaveContactToDatabaseAsync();
            App.Context.NotifyContactsAdded(new[] { new MailContactContext(contact) });
            App.Context.ClosePopup();
        }

        private async Task<MailContactModel> SaveContactToDatabaseAsync() {

            try {
                var contact = new MailContactModel {
                    Name = Name,
                    Address = Address,
                    SecurityMeasure = SecurityMeasure.None
                };

                using (var database = new DatabaseContext()) {
                    database.MailContacts.Add(contact);
                    await database.SaveChangesAsync();
                }

                return contact;
            }
            catch (Exception) {
                throw;
            }
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
