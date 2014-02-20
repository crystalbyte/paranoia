#region Using directives

using System.Diagnostics;

#endregion

namespace Crystalbyte.Paranoia.Messaging {
    [DebuggerDisplay("Name = {Name}, Address = {Address}")]
    public sealed class MailAddress {

        public MailAddress(string name, string address) 
            : this(address) {
            Name = name;
        }

        public MailAddress(string address) {
            Address = address;
        }

        public string Name { get; set; }
        public string Address { get; set; }
    }
}