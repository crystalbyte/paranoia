#region Using directives

using System;
using System.Composition;
using System.Net.Mail;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Crystalbyte.Paranoia.Contexts;

#endregion

namespace Crystalbyte.Paranoia.Commands {
    [Export, Shared]
    public sealed class ComposeMessageCommand : ICommand {

        #region Import Declarations

        [Import]
        public ContactSelectionSource ContactSelectionSource { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            ContactSelectionSource.SelectionChanged += 
                (sender, e) => OnCanExecuteChanged(EventArgs.Empty);
        }

        #endregion

        #region Implementation of ICommand

        public bool CanExecute(object parameter) {
            return ContactSelectionSource.Current != null;
        }

        public void Execute(object parameter) {
            MessageBox.Show("bla");

            //var message = new MailMessage
            //                  {From = new MailAddress("paranoia.app@gmail.com", "Paranoia Development", Encoding.UTF8)};

            //message.Headers.Add("x-p4-request", "1.0");
            //message.HeadersEncoding = Encoding.UTF8;

            //message.Subject = "p4r4n014 - Request";
            //message.SubjectEncoding = Encoding.UTF8;

            //message.To.Add(new MailAddress("paranoia.app@gmail.com", "Paranoia Development", Encoding.UTF8));
            //message.ReplyToList.Add(new MailAddress("paranoia.app@gmail.com", "Paranoia Development", Encoding.UTF8));

            //message.Body = "This is a communication request.";

            //var account = AppContext.SmtpAccounts.First();
            //using (var session = new SmtpSession(account.Host, account.Port,
            //                                     new SmtpCredentials
            //                                         {Username = account.Username, Password = account.Password})) {
            //    session.IsSslEnabled = true;
            //    await session.SendAsync(message);
            //}
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