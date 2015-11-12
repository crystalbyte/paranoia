using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI;
using System;

namespace Crystalbyte.Paranoia {
    public sealed class MailView : View {

        #region Private Fields

        private MailModule _module;

        #endregion

        #region Construction

        public MailView(MailModule module) {
            _module = module;

            Title = Resources.Messages;
            IconUri = new Uri("/assets/message.png", UriKind.Relative);
        }

        #endregion

        #region Methods

        public override Uri GetPageUri() {
            return typeof(MailPage).ToPageUri();
        }

        #endregion
    }
}
