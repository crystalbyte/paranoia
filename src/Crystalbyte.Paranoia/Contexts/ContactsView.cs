using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI;
using System;

namespace Crystalbyte.Paranoia {
    public sealed class ContactsView : View {

        #region Private Fields

        private readonly MailModule _module;

        #endregion

        #region Construction

        public ContactsView(MailModule module) {
            _module = module;

            Title = Resources.Contacts;
            IconUri = new Uri("/assets/contact.png", UriKind.Relative);
        }

        #endregion

        #region Methods

        public override Uri GetPageUri() {
            return typeof(ContactsPage).ToPageUri();
        }

        #endregion
    }
}
