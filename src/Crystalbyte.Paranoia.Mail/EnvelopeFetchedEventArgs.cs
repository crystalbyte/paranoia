using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crystalbyte.Paranoia.Mail {
    public sealed class EnvelopeFetchedEventArgs : EventArgs {

        private ImapEnvelope _envelope;
        public EnvelopeFetchedEventArgs(ImapEnvelope envelope) {
            _envelope = envelope;
        }

        public ImapEnvelope Envelope { 
            get { return _envelope; } 
        }
    }
}
