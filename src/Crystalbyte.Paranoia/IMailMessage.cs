using System;
using System.Collections.Generic;

namespace Crystalbyte.Paranoia {
    
    public interface IMailMessage {

        string Subject { get; }

        MailContactContext From { get; }

        MailContactContext PrimaryTo { get; }

        IEnumerable<MailContactContext> To { get; }

        IEnumerable<MailContactContext> SecondaryTo { get; }

        IEnumerable<MailContactContext> Cc { get; }

        IEnumerable<AttachmentContext> Attachments { get; }

        DateTime EntryDate { get; }

        double Progress { get; }

        bool IsLoading { get; }

        bool HasMultipleRecipients { get; }

        bool HasCarbonCopies { get; }

        bool IsInitialized { get; }
    }
}
