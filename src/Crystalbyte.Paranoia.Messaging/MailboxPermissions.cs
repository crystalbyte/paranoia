#region Using directives

using System;

#endregion

namespace Crystalbyte.Paranoia.Messaging {
    [Flags]
    public enum MailboxPermissions {
        Read = 0,
        Write
    }
}