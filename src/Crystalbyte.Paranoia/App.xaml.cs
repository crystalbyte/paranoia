#region Using directives

using System;
using System.Composition;
using System.Composition.Hosting;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.UI;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///   Interaction logic for App.xaml
    /// </summary>
    public partial class App {

        public static readonly string Name = "Paranoia";

        [Import]
        public static AppContext Context { get; set; }

        internal static CompositionHost Composition { get; set; }

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            Compose();
            InitEnvironment();
        }

        private async static void InitEnvironment() {
            var location = ConfigurationManager.AppSettings["DataDirectory"];
            if (string.IsNullOrEmpty(location)) {
                throw new Exception("Entry for the DataDirectory missing from configuration file.");
            }

            var directory = Environment.ExpandEnvironmentVariables(location);
            AppDomain.CurrentDomain.SetData("DataDirectory", directory);

#if DEBUG
            using (var context = new DatabaseContext()) {
                var repo = new Repository(context);

                var accounts = await repo.GetAccountsAsync();
                if (accounts.Length != 0) 
                    return;

                var account = new MailAccount {
                    Name = "Paranoia Dev Team",
                    Address = "paranoia.app@gmail.com",
                    ImapUsername = "paranoia.app@gmail.com",
                    ImapPassword = "p4r4n014",
                    ImapHost = "imap.gmail.com",
                    ImapPort = 993,
                    ImapSecurity = (byte)SecurityPolicy.Explicit,
                    SmtpHost = "smtp.gmail.com",
                    SmtpPort = 465,
                    SmtpSecurity = (byte)SecurityPolicy.Explicit,
                    UseImapCredentialsForSmtp = true,
                    SmtpRequiresAuthentication = true,
                };

                await repo.SaveAccountAsync(account);
            }
#endif
        }

        private void Compose() {
            var config = new ContainerConfiguration()
                .WithAssembly(typeof(App).Assembly);

            Composition = config.CreateContainer();
            Composition.SatisfyImports(this);
        }
    }
}