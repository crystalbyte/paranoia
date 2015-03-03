#region Using directives

using System;
using Crystalbyte.Paranoia.Data;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class SmtpRequestContext : SelectionObject {
        private readonly CompositionModel _model;

        public SmtpRequestContext(CompositionModel model) {
            _model = model;
        }

        public bool IsSubjectNilOrEmpty {
            get { return string.IsNullOrEmpty(Subject); }
        }

        public string ToNameOrAddress {
            get { return string.IsNullOrEmpty(ToName) ? ToAddress : ToName; }
        }

        public DateTime CompositionDate {
            get { return _model.Date; }
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

        public Int64 Id {
            get { return _model.Id; }
        }
    }
}