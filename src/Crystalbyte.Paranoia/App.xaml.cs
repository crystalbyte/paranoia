#region Using directives

using System;
using System.Composition;
using System.Composition.Hosting;
using System.Configuration;
using System.Data.Entity;
using System.Windows;
using Crystalbyte.Paranoia.Cryptography;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        public static readonly string Name = "Paranoia";

        [Import]
        public static AppContext Context { get; set; }

        internal static CompositionHost Composition { get; set; }

        protected override async void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            InitEnvironment();
            Compose();

#if DEBUG

            using (var context = new DatabaseContext()) {
                var accounts = await context.MailAccounts.ToArrayAsync();
                if (accounts.Length != 0)
                    return;

                var account = new MailAccountModel
                {
                    Name = "Paranoia Test Account",
                    Address = "paranoia.app@gmail.com",
                    ImapUsername = "paranoia.app@gmail.com",
                    ImapPassword = "p4r4n014",
                    ImapHost = "imap.gmail.com",
                    ImapPort = 993,
                    ImapSecurity = SecurityPolicy.Implicit,
                    SmtpHost = "smtp.gmail.com",
                    SmtpPort = 465,
                    SmtpSecurity = SecurityPolicy.Implicit,
                    UseImapCredentialsForSmtp = true,
                    SmtpRequiresAuthentication = true,
                };

                account.Contacts.Add(new MailContactModel {
                    Name = string.Format("p4r4n014"),
                    Address = "paranoia.app@gmail.com"
                });

                account.Contacts.Add(new MailContactModel {
                    Name = string.Format("Alexander Wieser"),
                    Address = "alexander.wieser@crystalbyte.de"
                });

                account.Contacts.Add(new MailContactModel {
                    Name = string.Format("Youtube"),
                    Address = "noreply@youtube.com"
                });

                account.Mailboxes.Add(new MailboxModel
                {
                    Type = MailboxType.All
                });
                account.Mailboxes.Add(new MailboxModel
                {
                    Type = MailboxType.Inbox
                });
                account.Mailboxes.Add(new MailboxModel
                {
                    Type = MailboxType.Trash
                });
                account.Mailboxes.Add(new MailboxModel
                {
                    Type = MailboxType.Sent
                });
                account.Mailboxes.Add(new MailboxModel
                {
                    Type = MailboxType.Draft
                });

                context.MailAccounts.Add(account);

                await context.SaveChangesAsync();
            }
#endif
        }

        private static void InitEnvironment() {
            var location = ConfigurationManager.AppSettings["DataDirectory"];
            if (string.IsNullOrEmpty(location)) {
                throw new Exception("Entry for the DataDirectory missing from configuration file.");
            }

            var directory = Environment.ExpandEnvironmentVariables(location);
            AppDomain.CurrentDomain.SetData("DataDirectory", directory);
            Sodium.InitNativeLibrary();
        }

        private void Compose() {
            var config = new ContainerConfiguration()
                .WithAssembly(typeof (App).Assembly);

            Composition = config.CreateContainer();
            Composition.SatisfyImports(this);
        }
    }
}