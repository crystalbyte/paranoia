#region Using directives

using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Composition;
using System.Composition.Hosting;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using Awesomium.Core;
using Crystalbyte.Paranoia.Automation;
using Crystalbyte.Paranoia.Cryptography;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using dotless.Core;
using NLog;
using LogLevel = Awesomium.Core.LogLevel;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App {

        #region Private Fields

        private const string MutexId = @"Local\7141BF12-D7A5-40FC-A1BF-7EE2846FA836";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Properties

        [Import]
        public static AppContext Context { get; set; }

        internal static CompositionHost Composition { get; private set; }

        public static string InspectionCss {
            get { return GetCssResource("/Resources/html.inspection.less"); }
        }

        public static string CompositionCss {
            get { return GetCssResource("/Resources/html.composition.less"); }
        }

        #endregion

        private static void RunFirstStartProcedure() {
            //Contest for eml file extension.
            // TODO: http://msdn.microsoft.com/en-us/library/windows/desktop/cc144160(v=vs.85).aspx#first_run_and_defaults

            Settings.Default.IsFirstStart = false;
            Settings.Default.Save();
        }

        public static string GetCssResource(string name) {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) {
                return "body {}";
            }
            var uri = new Uri(name, UriKind.Relative);
            var info = GetResourceStream(uri);
            if (info == null) {
                var error = string.Format(Paranoia.Properties.Resources.ResourceNotFoundException, uri, typeof(App).Assembly.FullName);
                throw new Exception(error);
            }

            string less;
            const string pattern = "%.+?%";
            using (var reader = new StreamReader(info.Stream)) {
                var text = reader.ReadToEnd();
                less = Regex.Replace(text, pattern, m => {
                    var key = m.Value.Trim('%');
                    string color;
                    var success = MetroColors.TryGetColorByName(key, out color);
                    if (!success) {
                        return "fuchsia";
                    }
                    // Drop the alpha channel.
                    return string.Format("#{0}",
                        color.Substring(3));
                });
            }

            return Less.Parse(less);
        }

#if DEBUG
        protected async override void OnStartup(StartupEventArgs e) {
#else
        protected override void OnStartup(StartupEventArgs e) {
#endif
            base.OnStartup(e);

            var success = TryCallingLiveProcess();
            if (success) {
                Current.Shutdown();
                return;
            }

            if (Settings.Default.IsFirstStart) {
                RunFirstStartProcedure();
            }

            InitEnvironment();
            RegisterComClassFactory();
            InitSodium();
            InitAwesomium();

            Compose();

            InitTheme();

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

        private static void RegisterComClassFactory() {
            ApplicationClassFactory.Register();
        }

        private static void RevokeComClassFactory() {
            ApplicationClassFactory.Revoke();
        }

        private static bool TryCallingLiveProcess() {
            var success = false;

            // Commence craziness.
            // http://stackoverflow.com/questions/229565/what-is-a-good-pattern-for-using-a-global-mutex-in-c

            bool isNew;
            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            using (var mutex = new Mutex(false, MutexId, out isNew, securitySettings)) {
                var hasHandle = false;
                try {
                    hasHandle = mutex.WaitOne(3000, false);
                    if (hasHandle == false)
                        throw new TimeoutException("Timeout waiting for exclusive access.");

                    try {
                        var type = Type.GetTypeFromProgID(ApplicationClassFactory.ProgId);
                        var application = (IApplication) Activator.CreateInstance(type);

                        success = true;
                    } catch (Exception ex) {
                        Logger.Error(ex);
                        success = false;
                    }

                } catch (TimeoutException ex) {
                    Logger.Error(ex);
                } catch (AbandonedMutexException ex) {
                    Logger.Error(ex);
                    hasHandle = true;
                } finally {
                    if (hasHandle)
                        mutex.ReleaseMutex();
                }
            }

            return success;
        }

        protected override void OnExit(ExitEventArgs e) {
            // Make sure we shutdown the core last.
            if (WebCore.IsInitialized)
                WebCore.Shutdown();
            
            RevokeComClassFactory();

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
                    RemoteDebuggingPort = 1337,
                    HomeURL = "http://www.awesomium.com".ToUri(),
                    LogPath = @".\starter.log",
                    LogLevel = LogLevel.Verbose
                });
            }
        }

        private static void InitSodium() {
            Sodium.InitNativeLibrary();
        }

        private static void InitTheme() {
            var theme = Settings.Default.CustomTheme;
            if (string.IsNullOrWhiteSpace(theme)) {
                return;
            }

            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var themePath = Path.Combine(localAppData, theme);
            if (!Directory.Exists(themePath)) {
                return;
            }

            ApplyCustomTheme(themePath);
        }

        private static void InitEnvironment() {
#if DEBUG
            Settings.Default.AcceptUntrustedCertificates = true;
#endif
            Thread.CurrentThread.Name = "Dispatcher Thread";

            ServicePointManager.ServerCertificateValidationCallback =
                delegate { return Settings.Default.AcceptUntrustedCertificates; };

            var location = ConfigurationManager.AppSettings["DataDirectory"];
            if (string.IsNullOrEmpty(location)) {
                throw new Exception("Entry for the DataDirectory missing from configuration file.");
            }

            var directory = Environment.ExpandEnvironmentVariables(location);
            AppDomain.CurrentDomain.SetData("DataDirectory", directory);
        }

        private static void ApplyCustomTheme(string path) {
            var file = new FileInfo(Path.Combine(path, "theme.info"));
            if (!file.Exists) {
                return;
            }

            var lines = File.ReadLines(file.FullName);
            foreach (var line in lines) {
                var split = line.Split(':');
            }

            //TODO: to be continued ...
        }

        private void Compose() {
            var config = new ContainerConfiguration()
                .WithAssembly(typeof(App).Assembly);

            Composition = config.CreateContainer();
            Composition.SatisfyImports(this);
        }
    }
}