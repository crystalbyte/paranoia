#region Using directives

using System;
using System.Composition;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Windows.Input;
using Crystalbyte.Paranoia.Contexts;
using Crystalbyte.Paranoia.Messaging;

#endregion

namespace Crystalbyte.Paranoia.Commands {
    [Export, Shared]
    public sealed class ComposeMessageCommand : ICommand {
        [Import]
        public AppContext AppContext { get; set; }

        #region Implementation of ICommand
         
        public bool CanExecute(object parameter) {
            return true;
        }

        public async void Execute(object parameter) {
            var message = new MailMessage
                              {From = new MailAddress("paranoia.app@gmail.com", "Paranoia Development", Encoding.UTF8)};

            message.Headers.Add("x-p4-request", "1.0");
            message.HeadersEncoding = Encoding.UTF8;

            message.Subject = "p4r4n014 - Request";
            message.SubjectEncoding = Encoding.UTF8;

            message.To.Add(new MailAddress("paranoia.app@gmail.com", "Paranoia Development", Encoding.UTF8));
            message.ReplyToList.Add(new MailAddress("paranoia.app@gmail.com", "Paranoia Development", Encoding.UTF8));

            message.Body = "This is a communication request.";

            var account = AppContext.SmtpAccounts.First();
            using (var session = new SmtpSession(account.Host, account.Port,
                                                 new SmtpCredentials
                                                     {Username = account.Username, Password = account.Password})) {
                session.IsSslEnabled = true;
                await session.SendAsync(message);
            }
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion
    }
}