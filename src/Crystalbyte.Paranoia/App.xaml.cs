#region Using directives

using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Composition;
using System.Composition.Hosting;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using CefSharp;
using Crystalbyte.Paranoia.Automation;
using Crystalbyte.Paranoia.Cryptography;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.Themes;
using Crystalbyte.Paranoia.UI;
using dotless.Core;
using NLog;
using WinApp = System.Windows.Application;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App {

        #region Private Fields

        private readonly ComServer _server;
        private const string MutexId = @"Local\7141BF12-D7A5-40FC-A1BF-7EE2846FA836";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public App() {
            _server = new ComServer(this);
        }

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
            // Contest for eml file extension.
            // TODO: http://msdn.microsoft.com/en-us/library/windows/desktop/cc144160(v=vs.85).aspx#first_run_and_defaults

            Settings.Default.IsFirstStart = false;
            Settings.Default.Save();
        }

        public static string GetCssResource(string path) {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) {
                return "body {}";
            }

            var uri = new Uri(path, UriKind.Relative);
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
                    var dictionary =
                        Current.Resources.MergedDictionaries.FirstOrDefault(
                            x => x.Contains(key)) ?? Current.Resources;

                    var resource = dictionary[key];
                    if (resource is SolidColorBrush) {
                        return string.Format("#{0}", (resource as SolidColorBrush).Color.ToString().Substring(3));
                    }

                    return resource != null ? resource.ToString() : "fuchsia";
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

#if DEBUG
            var commands = Environment.CommandLine;
            var message = string.Format("Arguments: {0}{1}{1}Pid: {2}{1}{1}Continue?", commands, Environment.NewLine, Process.GetCurrentProcess().Id);
            var result = MessageBox.Show(message, "Command Line Arguments", MessageBoxButton.YesNo);
            if (result.HasFlag(MessageBoxResult.No)) {
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                Shutdown(1);
            }


            //System.Windows.Automation.Automation.AddAutomationFocusChangedEventHandler(OnFocusChanged);
#endif
            var success = TryCallingLiveProcess();
            if (success) {
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                Shutdown(0);
            }

            if (Settings.Default.IsFirstStart) {
                RunFirstStartProcedure();
            }

            Compose();

            InitEnvironment();
            InitSodium();
            InitChromium();
            InitThemes();
            
            StartComServer();
            ProcessCommandLine();
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
                await database.SaveChangesAsync();

                Settings.Default.AcceptUntrustedCertificates = true;
            }
#endif
        }

#if DEBUG
        //private static void OnFocusChanged(object sender, AutomationFocusChangedEventArgs e) {
        //    Debug.WriteLine(AutomationElement.FocusedElement.Current.ClassName);
        //}
#endif

        private void StartComServer() {
            _server.Start();
        }

        private static bool TryCallingLiveProcess() {
            var success = false;

            // Commence craziness.
            // http://stackoverflow.com/questions/229565/what-is-a-good-pattern-for-using-a-global-mutex-in-c

            bool isNew;
            var allowEveryoneRule = new MutexAccessRule(
                new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            using (var mutex = new Mutex(false, MutexId, out isNew, securitySettings)) {
                var hasHandle = false;
                try {
                    hasHandle = mutex.WaitOne(3000, false);
                    if (hasHandle == false)
                        throw new TimeoutException("Timeout waiting for exclusive access.");

                    try {
                        var name = Process.GetCurrentProcess().ProcessName;
                        var processes = Process.GetProcessesByName(name);
                        if (processes.Length == 1) {
                            // Since we are the sole process no relaying is possible.
                            return false;
                        }

                        RelayCommandLine();

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

        private static void RelayCommandLine() {
            var arguments = Environment.GetCommandLineArgs();
            if (arguments.Length == 1) {
                return;
            }

            try {
                var info = new FileInfo(arguments[1]);
                if (!info.Exists)
                    return;

                var type = Type.GetTypeFromProgID(Automation.Application.ProgId);
                dynamic application = Activator.CreateInstance(type);
                application.OpenFile(arguments[1]);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private static void ProcessCommandLine() {
            var arguments = Environment.GetCommandLineArgs();
            if (arguments.Length == 1) {
                return;
            }

            try {
                var info = new FileInfo(arguments[1]);
                if (!info.Exists)
                    return;

                // Can't access the main window here directly since it is not yet created.
                DeferredActions.Push(() => {
                    Current.MainWindow.Loaded += async (sender, e) => {
                        Current.MainWindow.WindowState = WindowState.Minimized;
                        await Context.InspectMessageAsync(info);
                    };
                });
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        protected override void OnExit(ExitEventArgs e) {
            base.OnExit(e);

            try {
                StopComServer();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private static void InitChromium() {
            var settings = new CefSettings {
                Locale = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            };

            settings.RegisterScheme(new CefCustomScheme {
                SchemeName = "asset",
                IsStandard = true,
                SchemeHandlerFactory = new AssetSchemeHandlerFactory()
            });

            Cef.Initialize(settings);
        }

        private void StopComServer() {
            _server.Stop();
        }

        private static void InitSodium() {
            Sodium.InitNativeLibrary();
        }

        private void InitThemes() {
            var name = Settings.Default.Theme;

            var theme =
                Context.Themes.FirstOrDefault(x => string.Compare(name, x.GetName(),
                        StringComparison.InvariantCultureIgnoreCase) == 0) ??
                Context.Themes.First(x => x is LightTheme);

            ApplyTheme(theme);

            var color = ColorConverter.ConvertFromString(Settings.Default.Accent);
            if (color != null) {
                ApplyAccent((Color)color);
            }
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

        private void Compose() {
            var config = new ContainerConfiguration()
                .WithAssembly(typeof(App).Assembly)
                .WithAssembly(typeof(DarkTheme).Assembly)
                .WithAssembly(typeof(LightTheme).Assembly);

            Composition = config.CreateContainer();
            Composition.SatisfyImports(this);
        }

        public void ApplyTheme(Theme theme) {
            var resources = theme.GetThemeResources();

            // Add base styles.
            Current.Resources.MergedDictionaries.Clear();
            Current.Resources.MergedDictionaries.AddRange(resources);

            // Derived styles need to be added after base styles.
            var url = string.Format(Pack.Relative, typeof(App).Assembly.FullName, "/App.Styles.xaml");
            var styles = (ResourceDictionary)LoadComponent(new Uri(url, UriKind.Relative));
            Current.Resources.MergedDictionaries.Add(styles);

            Settings.Default.Theme = theme.Name;
            Settings.Default.Save();
        }

        public void ApplyAccent(Color color) {
            Current.Resources[ThemeResourceKeys.AppAccentBrushKey] = new SolidColorBrush(color);
            Settings.Default.Accent = color.ToHex(false);
            Settings.Default.Save();

            // BUG: Some BorderBrushes do not update after Resource change.
            // https://connect.microsoft.com/VisualStudio/feedback/details/589898/wpf-border-borderbrush-does-not-see-changes-in-dynamic-resource
            Current.Windows.OfType<IAccentAware>().ForEach(x => x.OnAccentChanged());
        }
    }
}