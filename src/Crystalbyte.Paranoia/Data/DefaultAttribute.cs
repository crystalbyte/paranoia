using System;

namespace Crystalbyte.Paranoia.Data {
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class DefaultAttribute : Attribute {
        public DefaultAttribute(DatabaseFunction function) {
            Function = function;
        }

        public DatabaseFunction Function { get; set; }
    }
}
