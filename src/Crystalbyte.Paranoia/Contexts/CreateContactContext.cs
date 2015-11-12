#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using Directives

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.UI.Commands;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class CreateContactContext : NotificationObject {

        #region Private Fields

        private string _name;
        private string _address;
        private readonly ICommand _createContactCommand;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public CreateContactContext() {
            _createContactCommand = new RelayCommand(OnCreateContact);
        }

        #endregion

        #region Properties

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

        #endregion

        #region Methods

        private async void OnCreateContact(object obj) {
            try {
                var contact = await StoreContactAsync();
                var module = App.Context.GetModule<MailModule>();
                module.NotifyContactsCreated(new[] {
                    new MailContactContext(contact)
                });
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            } finally {
                App.Context.ClosePopup();
            }
        }

        private async Task<MailContact> StoreContactAsync() {
            var contact = new MailContact {
                Name = Name,
                Address = Address
            };

            using (var database = new DatabaseContext()) {
                database.MailContacts.Add(contact);
                await database.SaveChangesAsync();
            }

            return contact;
        }

        #endregion
    }
}