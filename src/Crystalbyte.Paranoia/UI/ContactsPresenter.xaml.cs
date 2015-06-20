using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using NLog;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for ContactsPresenter.xaml
    /// </summary>
    public partial class ContactsPresenter {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public ContactsPresenter() {
            InitializeComponent();
        }

        #endregion

        public IEnumerable<MailAddressContext> Addresses {
            get { return (IEnumerable<MailAddressContext>)GetValue(AddressesProperty); }
            set { SetValue(AddressesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Addresses.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddressesProperty =
            DependencyProperty.Register("Addresses", typeof(IEnumerable<MailAddressContext>), typeof(ContactsPresenter), new PropertyMetadata(null));
    }
}
