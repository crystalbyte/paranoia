using System.Collections.Generic;
using System.Windows;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for ContactsPresenter.xaml
    /// </summary>
    public partial class ContactsPresenter {
        public ContactsPresenter() {
            InitializeComponent();
        }

        public IEnumerable<MailAddressContext> Addresses {
            get { return (IEnumerable<MailAddressContext>)GetValue(AddressesProperty); }
            set { SetValue(AddressesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Addresses.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddressesProperty =
            DependencyProperty.Register("Addresses", typeof(IEnumerable<MailAddressContext>), typeof(ContactsPresenter), new PropertyMetadata(null));
    }
}
