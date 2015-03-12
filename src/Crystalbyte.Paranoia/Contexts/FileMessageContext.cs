using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    public sealed class FileMessageContext : NotificationObject, IMailMessage {
        private readonly FileSystemInfo _file;

        public FileMessageContext(FileSystemInfo file) {
            _file = file;
        }

        public string FullName {
            get { return _file.FullName; }
        }

        internal Task LoadAsync() {
            throw new NotImplementedException();
        }

        #region Implementation of IMailMessage

        public string Subject { get; private set; }
        public MailContactContext From { get; private set; }
        public MailContactContext PrimaryTo { get; private set; }
        public IEnumerable<MailContactContext> To { get; private set; }
        public IEnumerable<MailContactContext> SecondaryTo { get; private set; }
        public IEnumerable<MailContactContext> Cc { get; private set; }
        public IEnumerable<AttachmentContext> Attachments { get; private set; }
        public DateTime EntryDate { get; private set; }
        public double Progress { get; private set; }
        public bool IsLoading { get; private set; }
        public bool HasMultipleRecipients { get; private set; }
        public bool HasCarbonCopies { get; private set; }
        public bool IsInitialized { get; private set; }

        #endregion
    }
}
