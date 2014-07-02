#region Using directives

using System;

#endregion

namespace Crystalbyte.Paranoia.Mail {
    [Flags]
    public enum MailboxPermissions {
        Read = 0,
        Write
    }
}