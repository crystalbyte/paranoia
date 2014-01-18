using System.Diagnostics;
using System.Net.Mail;
using System.Net.Mime;
using Crystalbyte.Paranoia.Messaging;
using System;
using System.Composition;
using System.Threading.Tasks;
using System.Windows.Input;
using Crystalbyte.Paranoia.Messaging.Mime;
using Crystalbyte.Paranoia.Properties;
using System.Windows;
using System.IO;
using System.Text;

namespace Crystalbyte.Paranoia.Commands {
    [Export, Shared]
    public sealed class KeyExchangeCommand : ICommand {

        #region Import Declarations

        [Import]
        public IdentitySelectionSource IdentitySelectionSource { get; set; }

        [Import]
        public ContactSelectionSource ContactSelectionSource { get; set; }

        [Import]
        public ImapAccountSelectionSource ImapAccountSelectionSource { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            IdentitySelectionSource.SelectionChanged += (sender, e) => OnCanExecuteChanged(EventArgs.Empty);
            ContactSelectionSource.SelectionChanged += (sender, e) => OnCanExecuteChanged(EventArgs.Empty);
        }

        #endregion

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return IdentitySelectionSource.Current != null
                && ContactSelectionSource.Current != null
                && ContactSelectionSource.Current.RequestStatus != ContactRequestStatus.Accepted;
        }

        public void Execute(object parameter) {
            SendKeyExchangeAsync();
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        private async void SendKeyExchangeAsync() {
            var contact = ContactSelectionSource.Current;
            var identity = IdentitySelectionSource.Current;
            var account = ImapAccountSelectionSource.Current;
            var host = account.SmtpHost;
            var port = account.SmtpPort;

            using (var connection = new SmtpConnection()) {
                using (var authenticator = await connection.ConnectAsync(host, port)) {

                    using (var session = await authenticator.LoginAsync(account.SmtpUsername, account.SmtpPassword)) {

                        var message = new MailMessage {
                            HeadersEncoding = Encoding.UTF8,
                            SubjectEncoding = Encoding.UTF8,
                            IsBodyHtml = true,
                            BodyTransferEncoding = TransferEncoding.Base64
                        };

                        message.Headers.Add(ParanoiaHeaders.Version, "1.0");
                        message.Headers.Add(ParanoiaHeaders.KeyExchange, string.Format("{0} <{1}>", identity.Name, identity.EmailAddress));

                        var key = new MemoryStream(Encoding.UTF8.GetBytes(identity.PublicKey));
                        message.Attachments.Add(new Attachment(key, "public-key", "text/plain"));

                        var name = contact.Model.Identity.Name;
                        message.Subject = string.Format(Resources.InvitationSubjectTemplate, name);

                        var info = Application.GetResourceStream(new Uri("Resources/invitation.html", UriKind.Relative));

                        Debug.Assert(info != null);

                        using (var reader = new StreamReader(info.Stream)) {
                            message.Body = await reader.ReadToEndAsync();
                        }

                        message.To.Add(new MailAddress(contact.EmailAddress, contact.Name));
                        message.From = new MailAddress(contact.Model.Identity.EmailAddress, contact.Model.Identity.Name);

                        await session.SendAsync(message);
                    }
                }
            }
        }
    }
}
