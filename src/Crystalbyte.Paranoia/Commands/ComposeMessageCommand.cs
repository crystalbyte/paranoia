#region Using directives

using System;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Crystalbyte.Paranoia.Contexts;
using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Properties;
using MailMessage = System.Net.Mail.MailMessage;

#endregion

namespace Crystalbyte.Paranoia.Commands {
    [Export, Shared]
    [Export(typeof(IAppBarCommand))]
    public sealed class ComposeMessageCommand : IAppBarCommand {

        #region Private Fields

        private BitmapImage _image;

        #endregion

        #region Import Declarations

        [Import]
        public IdentitySelectionSource IdentitySelectionSource { get; set; }

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
            return ContactSelectionSource.Contact != null;
        }

        public async void Execute(object parameter) {
            var identity = IdentitySelectionSource.Identity;
            var account = identity.SmtpAccount;
            var contact = ContactSelectionSource.Contact;

            using (var message = new MailMessage()) {
                message.From = new MailAddress(identity.Address, identity.Name);

                message.Headers.Add(MailHeaders.FromName, identity.Name);
                message.Headers.Add(MailHeaders.FromAddress, identity.Address);
                message.Headers.Add(MailHeaders.Type, MailTypes.Message);
                message.HeadersEncoding = Encoding.UTF8;

                message.Subject = "p4r4n014 - Html Sample Page";
                message.SubjectEncoding = Encoding.UTF8;

                message.To.Add(new MailAddress(contact.Address, contact.Name, Encoding.UTF8));
                message.ReplyToList.Add(new MailAddress(identity.Address, identity.Name));

                message.IsBodyHtml = true;
                var info = Application.GetResourceStream(new Uri("Resources/html-sample.html", UriKind.Relative));

                Debug.Assert(info != null);

                using (var reader = new StreamReader(info.Stream)) {
                    message.Body = await reader.ReadToEndAsync();
                }

                using (var connection = new SmtpConnection {Security = account.Security}) {
                    using (var authenticator = await connection.ConnectAsync(account.Host, account.Port)) {
                        using (var session = await authenticator.LoginAsync(account.Username, account.Password)) {
                            await session.SendAsync(message);
                        }    
                    }
                }
            }
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecuteChanged(EventArgs e) {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        #region Implementation of IAppBarCommand

        public string Tooltip {
            get { return Resources.ComposeMessageCommandTooltip; }
        }

        public string Category {
            get { return AppBarCategory.Contacts; }
        }

        public ImageSource Image {
            get {
                if (_image == null) {
                    var uri =
                        new Uri(string.Format(Pack.Application, typeof(InviteContactCommand).Assembly.FullName,
                                              "Assets/mail.png"), UriKind.Absolute);
                    _image = new BitmapImage(uri);
                }
                return _image;
            }
        }

        public int Position {
            get { return 0; }
        }

        #endregion
    }
}