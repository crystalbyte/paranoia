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
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using NavigationCommands = Crystalbyte.Paranoia.UI.FlyoutCommands;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for ContactsPage.xaml
    /// </summary>
    public partial class ContactsPage {
        public ContactsPage() {
            InitializeComponent();
            DataContext = App.Context;
            CommandBindings.Add(new CommandBinding(AppCommands.JumpToContact, OnJumpToContact, OnCanJumpToContact));
        }

        private static void OnCanJumpToContact(object sender, CanExecuteRoutedEventArgs e) {
            var button = e.OriginalSource as Button;
            if (button == null) {
                return;
            }

            var value = (char) button.DataContext;
            var contact = App.Context.Contacts.FirstOrDefault(
                x => x.Name.StartsWith(new string(value, 1), StringComparison.InvariantCultureIgnoreCase));
            e.CanExecute = contact != null;
        }

        private void OnJumpToContact(object sender, ExecutedRoutedEventArgs e) {
            var button = e.OriginalSource as Button;
            if (button == null) {
                return;
            }

            var value = (char) button.DataContext;
            var contact = App.Context.Contacts
                .Where(x => !x.Name.StartsWith("NIL", StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(x => x.Name)
                .FirstOrDefault(
                    x => x.Name.StartsWith(new string(value, 1), StringComparison.InvariantCultureIgnoreCase));

            if (contact != null) {
                ContactsList.ScrollToCenterOfView(contact);
            }
        }

        private void OnContactsSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!IsLoaded) {
                return;
            }

            var app = App.Context;
            app.OnContactSelectionChanged();

            var contact = app.SelectedContact;
            if (contact == null)
                return;

            var container = (Control) ContactsList.ItemContainerGenerator.ContainerFromItem(contact);
            if (container != null) {
                container.Focus();
            }
        }
    }
}