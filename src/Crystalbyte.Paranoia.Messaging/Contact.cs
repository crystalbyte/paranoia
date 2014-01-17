#region Using directives

using System.Diagnostics;

#endregion

namespace Crystalbyte.Paranoia.Messaging {
    [DebuggerDisplay("Name = {Name}, Address = {Address}")]
    public sealed class Contact {
        public string Name { get; set; }
        public string Address { get; set; }
    }
}