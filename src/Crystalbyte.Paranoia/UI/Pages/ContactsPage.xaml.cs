using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI.Pages {
    /// <summary>
    /// Interaction logic for ContactsPage.xaml
    /// </summary>
    public partial class ContactsPage {
        public ContactsPage() {
            InitializeComponent();
            DataContext = App.Context;

            CommandBindings.Add(new CommandBinding(PageCommands.ScrollToLetter, OnScrollToLetter, OnCanScrollToLetter));
        }

        private void OnCanScrollToLetter(object sender, CanExecuteRoutedEventArgs e) {
            var button = e.OriginalSource as Button;
            if (button == null) {
                return;
            }

            var value = (char)button.DataContext;
            var contact = App.Context.Contacts.FirstOrDefault(
                x => x.Name.StartsWith(new string(value, 1), StringComparison.InvariantCultureIgnoreCase));
            e.CanExecute = contact != null;
        }

        private void OnScrollToLetter(object sender, ExecutedRoutedEventArgs e) {
            var button = e.OriginalSource as Button;
            if (button == null) {
                return;
            }

            var value = (char)button.DataContext;

            var contact = App.Context.Contacts
                .Where(x => !x.Name.StartsWith("NIL", StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault(x => x.Name.StartsWith(new string(value, 1), StringComparison.InvariantCultureIgnoreCase));

            if (contact != null)
                ContactsList.ScrollToCenterOfView(contact);
        }
    }
}
