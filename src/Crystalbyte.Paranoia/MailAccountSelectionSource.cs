﻿using System.Composition;

namespace Crystalbyte.Paranoia {
    [Export, Shared]
    public sealed class MailAccountSelectionSource 
        : SelectionSource<MailAccountContext> { }
}
