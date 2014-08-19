#region Using directives

using System;

#endregion

namespace Crystalbyte.Paranoia.Data {
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal sealed class CollateAttribute : Attribute {
        private readonly CollatingSequence _sequence;

        public CollateAttribute(CollatingSequence sequence) {
            _sequence = sequence;
        }

        public CollatingSequence Sequence {
            get { return _sequence; }
        }
    }
}