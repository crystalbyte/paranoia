﻿#region Using directives

using System.Diagnostics;

#endregion

namespace Crystalbyte.Paranoia.Messaging {
    [DebuggerDisplay("Alias = {Alias}, Address = {Address}")]
    public sealed class MailContact {
        public string Alias { get; set; }
        public string Address { get; set; }
    }
}