using System;

namespace Crystalbyte.Paranoia {
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DescriptionAttribute : Attribute {
        public Type Type { get; set; }

        public string Name { get; set; }
    }
}
