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

using NLog;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for ContactsPage.xaml
    /// </summary>
    public partial class ContactsPage {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ContactsPage() {
            InitializeComponent();
            Loaded += OnPageLoaded;
            Unloaded += OnPageUnloaded;
            DataContext = App.Context.GetModule<MailModule>();
        }

        private MailModule GetContext() {
            if (DataContext == null) {
                throw new NullReferenceException("DataContext");
            }

            return (MailModule) DataContext;
        }

        private void OnPageUnloaded(object sender, RoutedEventArgs e) {
            try {
                var context = GetContext();
                //App.Context.UnloadContacts();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e) {
            try {
                var context = GetContext();
                //await App.Context.LoadContactsAsync();
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }
        }

        private void OnCanJumpToContact(object sender, CanExecuteRoutedEventArgs e) {
            var button = e.OriginalSource as Button;
            if (button == null) {
                return;
            }

            var context = GetContext();
            var value = (char)button.DataContext;
            var contact = context.Contacts.FirstOrDefault(
                x => x.Name.StartsWith(new string(value, 1), StringComparison.InvariantCultureIgnoreCase));
            e.CanExecute = contact != null;
        }

        private void OnJumpToContact(object sender, ExecutedRoutedEventArgs e) {
            var button = e.OriginalSource as Button;
            if (button == null) {
                return;
            }

            var context = GetContext();
            var value = (char)button.DataContext;
            var contact = context.Contacts
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

            var context = GetContext();
            context.OnContactSelectionChanged();

            var contact = context.SelectedContact;
            if (contact == null)
                return;

            var container = (Control)ContactsList.ItemContainerGenerator.ContainerFromItem(contact);
            if (container != null) {
                container.Focus();
            }
        }
    }
}