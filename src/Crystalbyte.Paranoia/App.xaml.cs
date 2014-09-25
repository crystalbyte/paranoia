﻿#region Using directives

using System;
using System.Data.Entity;
using System.Composition;
using System.Composition.Hosting;
using System.Configuration;
using System.Net;
using System.Threading;
using System.Windows;
using Awesomium.Core;
using Crystalbyte.Paranoia.Cryptography;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        public static readonly string Name = "Paranoia";

        [Import]
        public static AppContext Context { get; set; }

        internal static CompositionHost Composition { get; private set; }

        public static ResourceDictionary ThemeDictionary { get; set; }

#if DEBUG
        protected async override void OnStartup(StartupEventArgs e) {

            Settings.Default.AcceptUntrustedCertificates = true;
#else
 protected override void OnStartup(StartupEventArgs e) {
#endif

            base.OnStartup(e);

            Thread.CurrentThread.Name = "Dispatcher Thread";

            ServicePointManager.ServerCertificateValidationCallback =
                delegate { return Settings.Default.AcceptUntrustedCertificates; };

            InitEnvironment();
            InitSodium();
            InitAwesomium();

            Compose();

#if DEBUG

            using (var database = new DatabaseContext()) {
                var accounts = await database.MailAccounts.ToArrayAsync();
                if (accounts.Length != 0)
                    return;

                var account = new MailAccountModel {
                    Name = "Paranoia Test Account",
                    Address = "paranoia.app@gmail.com",
                    ImapUsername = "paranoia.app@gmail.com",
                    ImapPassword = "p4r4n014",
                    ImapHost = "imap.gmail.com",
                    ImapPort = 993,
                    ImapSecurity = SecurityProtocol.Implicit,
                    SmtpHost = "smtp.gmail.com",
                    SmtpPort = 465,
                    SmtpUsername = "paranoia.app@gmail.com",
                    SmtpPassword = "p4r4n014",
                    SmtpSecurity = SecurityProtocol.Implicit,
                    UseImapCredentialsForSmtp = true,
                    SmtpRequiresAuthentication = true,
                };

                database.MailAccounts.Add(account);

                account = new MailAccountModel {
                    Name = "SPAM",
                    Address = "osemc_test@organice.de",
                    ImapUsername = "osemc_test",
                    ImapPassword = "dreissig",
                    ImapHost = "mail.organice.de",
                    ImapPort = 993,
                    ImapSecurity = SecurityProtocol.Implicit,
                    SmtpHost = "mail.organice.de",
                    SmtpPort = 25,
                    SmtpUsername = "osemc_test",
                    SmtpPassword = "dreissig",
                    SmtpSecurity = SecurityProtocol.Explicit,
                    UseImapCredentialsForSmtp = true,
                    SmtpRequiresAuthentication = true,
                };

                Settings.Default.AcceptUntrustedCertificates = true;

                database.MailAccounts.Add(account);

                await database.SaveChangesAsync();
            }
#endif
        }

        protected override void OnExit(ExitEventArgs e) {
            // Make sure we shutdown the core last.
            if (WebCore.IsInitialized)
                WebCore.Shutdown();

            base.OnExit(e);
        }

        private static void InitAwesomium() {

            WebCore.Initialized += (sender, e) => {
                WebCore.ResourceInterceptor = new ResourceInterceptor();
            };

            // Initialization must be performed here,
            // before creating a WebControl.
            if (!WebCore.IsInitialized) {
                WebCore.Initialize(new WebConfig {
                    HomeURL = "http://www.awesomium.com".ToUri(),
                    LogPath = @".\starter.log",
                    LogLevel = LogLevel.Verbose
                });
            }
        }

        private static void InitSodium() {
            Sodium.InitNativeLibrary();
        }

        private static void InitEnvironment() {
            var location = ConfigurationManager.AppSettings["DataDirectory"];
            if (string.IsNullOrEmpty(location)) {
                throw new Exception("Entry for the DataDirectory missing from configuration file.");
            }

            var directory = Environment.ExpandEnvironmentVariables(location);
            AppDomain.CurrentDomain.SetData("DataDirectory", directory);

            ThemeDictionary = new ResourceDictionary {
                Source = new Uri("/Themes/Metro.Resources.xaml", UriKind.Relative)
            };
        }

        private void Compose() {
            var config = new ContainerConfiguration()
                .WithAssembly(typeof(App).Assembly);

            Composition = config.CreateContainer();
            Composition.SatisfyImports(this);
        }
    }
}