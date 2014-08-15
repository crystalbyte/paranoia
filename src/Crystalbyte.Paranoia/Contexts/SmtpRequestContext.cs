using Crystalbyte.Paranoia.Data;

namespace Crystalbyte.Paranoia {
    public sealed class SmtpRequestContext : SelectionObject {
        private readonly SmtpRequestModel _model;

        public SmtpRequestContext(SmtpRequestModel model) {
            _model = model;
        }

        public string Subject {
            get { return _model.Subject; }
        }

        public string ToName {
            get { return _model.ToName; }
        }

        public string ToAddress {
            get { return _model.ToAddress; }
        }
    }
}
