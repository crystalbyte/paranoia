using System.Collections.ObjectModel;
using Crystalbyte.Paranoia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Crystalbyte.Paranoia.Data;
using NLog;
using Crystalbyte.Paranoia.Messaging;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;
using Crystalbyte.Paranoia.Properties;
using System.Windows;
using System.Diagnostics;

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class ContactContext : NotificationObject {

        #region Private Fields

        private readonly Contact _contact;
        private string _gravatarImageUrl;
        private bool _isSelected;

        #endregion

        #region Construction

        public ContactContext()
            : this(new Contact()) {
        }

        public ContactContext(Contact contact) {
            _contact = contact;
        }

        #endregion

        #region Log Declaration

        private static Logger Log = LogManager.GetCurrentClassLogger();

        #endregion

        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) {
                    return;
                }

                RaisePropertyChanging(() => IsSelected);
                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }

        public int Id {
            get { return _contact.Id; } 
        }

        public string Name {
            get { return _contact.Name; }
        }

        public string Address {
            get { return _contact.Address; }
        }

        public ContactRequest ContactRequest {
            get { return _contact.ContactRequest; }
        }

        public string GravatarUrl {
            get {
                if (string.IsNullOrWhiteSpace(_gravatarImageUrl)) {
                    CreateGravatarImageUrl();
                }
                return _gravatarImageUrl;
            }
            set {
                if (_gravatarImageUrl == value) {
                    return;
                }

                RaisePropertyChanging(() => GravatarUrl);
                _gravatarImageUrl = value;
                RaisePropertyChanged(() => GravatarUrl);
            }
        }

        private void CreateGravatarImageUrl() {
            GravatarUrl = Gravatar.CreateImageUrl(Address);
        }

        internal async Task DeleteAsync() {
            // http://blogs.msdn.com/b/adonet/archive/2013/08/21/ef6-release-candidate-available.aspx
            await Task.Factory.StartNew(() => {
                try {
                    using (var context = new StorageContext()) {
                        var c = context.Contacts.Find(_contact.Id);
                        if (c != null) {
                            context.Contacts.Remove(c);
                            context.SaveChanges();
                        }
                    }
                } catch (Exception ex) {
                    Log.Error(ex.Message);
                }
            });
        }

        internal async Task SendInviteAsync() {
            Identity identity = null;
            SmtpAccount account = null;
            await Task.Factory.StartNew(() => {
                try {
                    using (var context = new StorageContext()) {
                        identity = context.Identities.Find(_contact.IdentityId);
                        account = identity.SmtpAccount;
                    }
                } catch (Exception ex) {
                    Log.Error(ex.Message);
                }
            });

            if (identity == null || account == null) {
                Log.Error("identity == null || account == null");
                return;
            }

            using (var connection = new SmtpConnection()) {
                using (var authenticator = await connection.ConnectAsync(account.Host, account.Port)) {
                    using (var session = await authenticator.LoginAsync(account.Username, account.Password)) {
                        var message = new System.Net.Mail.MailMessage {
                            HeadersEncoding = Encoding.UTF8,
                            SubjectEncoding = Encoding.UTF8,
                            IsBodyHtml = true,
                            BodyTransferEncoding = TransferEncoding.Base64
                        };

                        message.Headers.Add(MessageHeaders.FromName, identity.Name);
                        message.Headers.Add(MessageHeaders.FromAddress, identity.Address);
                        message.Headers.Add(MessageHeaders.Type, MessageTypes.Request);

                        var key = new MemoryStream(Encoding.UTF8.GetBytes("public-key"));
                        message.Attachments.Add(new Attachment(key, "public-key", "text/plain"));

                        var name = identity.Name;
                        message.Subject = string.Format(Resources.InvitationSubjectTemplate, name);

                        var info = Application.GetResourceStream(new Uri("Resources/invitation.html", UriKind.Relative));

                        Debug.Assert(info != null);

                        using (var reader = new StreamReader(info.Stream)) {
                            message.Body = await reader.ReadToEndAsync();
                        }

                        message.To.Add(new MailAddress(Address, Name));
                        message.From = new MailAddress(identity.Address, identity.Name);

                        try {
                            await session.SendAsync(message);
                        } catch (Exception ex) {
                            Log.Error(ex.Message);
                        }
                    }
                }
            }
        }
    }
}
