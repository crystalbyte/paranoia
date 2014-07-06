using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using System.Collections.Generic;

namespace Crystalbyte.Paranoia {
    public sealed class ImapMailboxContext : SelectionObject {
        private readonly MailboxModel _mailbox;
        private Exception _lastException;

        internal ImapMailboxContext(MailboxModel mailbox) {
            _mailbox = mailbox;
        }

        public string Name {
            get { return _mailbox.Name; }
            set {
                if (_mailbox.Name == value) {
                    return;
                }

                _mailbox.Name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public Exception LastException {
            get { return _lastException; }
            set {
                if (_lastException == value) {
                    return;
                }

                _lastException = value;
                RaisePropertyChanged(() => LastException);
            }
        }

        public bool IsAssigned {
            get { return !string.IsNullOrEmpty(Name); }
        }

        public MailboxType Type {
            get { return _mailbox.Type; }
        }

        internal Task SyncAsync() {
            throw new NotImplementedException();
        }

        internal async Task AssignMostProbableAsync(List<ImapMailboxInfo> remoteMailboxes) {
            switch (Type) {
                case MailboxType.Inbox:
                    await AssignAsync(remoteMailboxes.SingleOrDefault(
                            x => CultureInfo.CurrentCulture.CompareInfo
                                .IndexOf(x.Name, "inbox", CompareOptions.IgnoreCase) >= 0));
                    break;
                case MailboxType.Sent:
                    await AssignAsync(remoteMailboxes.SingleOrDefault(
                            x => CultureInfo.CurrentCulture.CompareInfo
                                .IndexOf(x.Name, "sent", CompareOptions.IgnoreCase) >= 0));
                    break;
                case MailboxType.Draft:
                    await AssignAsync(remoteMailboxes.SingleOrDefault(
                            x => CultureInfo.CurrentCulture.CompareInfo
                                .IndexOf(x.Name, "draft", CompareOptions.IgnoreCase) >= 0));
                    break;
                case MailboxType.Trash:
                    await AssignAsync(remoteMailboxes.SingleOrDefault(
                            x => CultureInfo.CurrentCulture.CompareInfo
                                .IndexOf(x.Name, "trash", CompareOptions.IgnoreCase) >= 0));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal async Task AssignAsync(ImapMailboxInfo mailbox) {
            try {
                // If no match has been found mailbox will be null.
                if (mailbox == null) {
                    return;
                }

                using (var context = new DatabaseContext()) {
                    context.Mailboxes.Attach(_mailbox);

                    _mailbox.Name = mailbox.Fullname;
                    _mailbox.Delimiter = mailbox.Delimiter;
                    _mailbox.Flags = mailbox.Flags.Aggregate((c, n) => c + ';' + n);

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex) {
                LastException = ex;
            }
        }
    }
}
