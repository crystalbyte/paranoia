#region Using directives

using System;
using System.ComponentModel.DataAnnotations;
using System.Composition;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Xml.Serialization;
using Crystalbyte.Paranoia.Commands;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Models;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI;
using System.Diagnostics;
using System.Text;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    [Export, Shared]
    public sealed class CreateAccountScreenContext : ValidationObject<CreateAccountScreenContext> {

        #region Private Fields

        private const int ShiftIn = 4096;
        private const int ShiftOut = -4096;

        private bool _isActive;
        private short _imapSecurity;
        private string _imapHost;
        private short _imapPort;
        private string _imapUsername;
        private string _imapPassword;
        private string _smtpPassword;
        private string _smtpUsername;
        private string _smtpHost;
        private short _smtpSecurity;
        private bool _isRequesting;
        private bool _isTesting;
        private short _smtpPort;
        private string _emailAddress;
        private bool _isTestSuccessful;
        private SettingsTestContext _networkTest;
        private SettingsTestContext _imapConnectionTest;
        private SettingsTestContext _smtpConnectionTest;

        #endregion

        #region Construction

        public CreateAccountScreenContext() {
            CancelCommand = new RelayCommand(OnCancel);
            TestingCommand = new RelayCommand(OnCanTest, OnTest);
            ConfigCommand = new RelayCommand(OnCanConfigure, OnConfigure);
            GoBackCommand = new RelayCommand(OnGoBack);
            CommitCommand = new RelayCommand(OnCanCommit, OnCommit);
        }

        #endregion

        #region Import Declarations

        [Import]
        public AppContext AppContext { get; set; }

        [Import]
        public IdentitySelectionSource IdentitySelectionSource { get; set; }

        [Import]
        public StorageContext LocalStorage { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            // Nada    
        }

        #endregion

        private async void OnCommit(object obj) {
            var account = new SmtpAccount {
                Name = ImapHost,
                Name = EmailAddress,
                Port = ImapPort,
                Password = ImapPassword,
                Username = ImapUsername,
                Security = (short)ImapSecurity,
                SmtpAccount = new SmtpAccount {
                    Host = SmtpHost,
                    Port = SmtpPort,
                    Security = (short)SmtpSecurity,
                    Username = SmtpUsername,
                    Password = SmtpPassword
                }
            };

            try {
                AppContext.ImapAccounts.Add(new ImapAccountContext(account));
                await Task.Factory.StartNew(() => {
                    LocalStorage.Context.ImapAccounts.Add(account);
                    LocalStorage.Context.SaveChanges();
                });
            }
            catch (Exception) {
                // TODO: Errorlog
                throw;
            }
            finally {
                IsActive = false;
            }
        }

        private bool OnCanCommit(object arg) {
            return IsValid
                && NetworkTest != null && NetworkTest.IsSuccessful
                && ImapConnectionTest != null && ImapConnectionTest.IsSuccessful
                && SmtpConnectionTest != null && SmtpConnectionTest.IsSuccessful;
        }

        private static void OnGoBack(object obj) {
            var service = NavigationService.GetNavigationService((DependencyObject)obj);
            if (service == null)
                return;

            service.GoBack();
        }

        private async void OnConfigure(object obj) {
            await AutoConfigAsync();
            NavigateToConfigPage(obj as DependencyObject);
        }

        private static void NavigateToConfigPage(DependencyObject page) {
            var service = NavigationService.GetNavigationService(page);
            if (service == null)
                return;

            var uri = string.Format("/UI/{0}.xaml", typeof(CreateAccountConfigPage).Name);
            service.Navigate(new Uri(uri, UriKind.Relative));
        }

        private bool OnCanConfigure(object arg) {
            return !string.IsNullOrWhiteSpace(EmailAddress)
                   && Regex.IsMatch(EmailAddress, RegexPatterns.Email)
                   && !IsRequesting;
        }

        private bool OnCanTest(object arg) {
            return IsValid;
        }

        private void OnCancel(object obj) {
            IsActive = false;
        }

        private static void OnTest(object obj) {
            NavigateToTestingPage(obj as DependencyObject);
        }

        private static void NavigateToTestingPage(DependencyObject page) {
            var service = NavigationService.GetNavigationService(page);
            if (service == null)
                return;

            var uri = string.Format("/UI/{0}.xaml", typeof(CreateAccountTestingPage).Name);
            service.Navigate(new Uri(uri, UriKind.Relative));
        }

        #region ValidationObject Overrides

        protected override void OnValidating(EventArgs e) {
            base.OnValidating(e);
            InvalidateCommands();
        }

        #endregion

        #region Event Declarations

        public event EventHandler Activated;

        public void OnActivated(EventArgs e) {
            var handler = Activated;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        public ICommand CancelCommand { get; private set; }
        public RelayCommand TestingCommand { get; private set; }
        public RelayCommand ConfigCommand { get; private set; }
        public RelayCommand GoBackCommand { get; private set; }
        public RelayCommand CommitCommand { get; private set; }

        public SettingsTestContext NetworkTest {
            get { return _networkTest; }
            set {
                if (_networkTest == value) {
                    return;
                }

                RaisePropertyChanging(() => NetworkTest);
                _networkTest = value;
                RaisePropertyChanged(() => NetworkTest);
            }
        }

        public SettingsTestContext ImapConnectionTest {
            get { return _imapConnectionTest; }
            set {
                if (_imapConnectionTest == value) {
                    return;
                }

                RaisePropertyChanging(() => ImapConnectionTest);
                _imapConnectionTest = value;
                RaisePropertyChanged(() => ImapConnectionTest);
            }
        }

        public SettingsTestContext SmtpConnectionTest {
            get { return _smtpConnectionTest; }
            set {
                if (_smtpConnectionTest == value) {
                    return;
                }

                RaisePropertyChanging(() => SmtpConnectionTest);
                _smtpConnectionTest = value;
                RaisePropertyChanged(() => SmtpConnectionTest);
            }
        }

        public bool IsActive {
            get { return _isActive; }
            set {
                if (_isActive == value) {
                    return;
                }

                RaisePropertyChanging(() => IsActive);
                _isActive = value;
                RaisePropertyChanged(() => IsActive);

                if (value) {
                    OnActivated(EventArgs.Empty);
                }
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        [RegularExpression(RegexPatterns.Email, ErrorMessageResourceType = typeof(Resources),
            ErrorMessageResourceName = "InvalidEmailFormatErrorText")]
        public string EmailAddress {
            get { return _emailAddress; }
            set {
                if (_emailAddress == value) {
                    return;
                }

                RaisePropertyChanging(() => EmailAddress);
                _emailAddress = value;
                RaisePropertyChanged(() => EmailAddress);
                InvalidateCommands();
            }
        }

        public string ImapPassword {
            get { return _imapPassword.Caesar(ShiftOut); }
            set {
                if (_imapPassword == value.Caesar(ShiftIn)) {
                    return;
                }

                RaisePropertyChanging(() => ImapPassword);
                _imapPassword = value.Caesar(ShiftIn);
                RaisePropertyChanged(() => ImapPassword);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        public string ImapHost {
            get { return _imapHost; }
            set {
                if (_imapHost == value) {
                    return;
                }

                RaisePropertyChanging(() => ImapHost);
                _imapHost = value;
                RaisePropertyChanged(() => ImapHost);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        public SecurityPolicy ImapSecurity {
            get { return (SecurityPolicy)_imapSecurity; }
            set {
                if (_imapSecurity == (short)value) {
                    return;
                }

                RaisePropertyChanging(() => ImapSecurity);
                _imapSecurity = (short)value;
                RaisePropertyChanged(() => ImapSecurity);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        public short ImapPort {
            get { return _imapPort; }
            set {
                if (_imapPort == value) {
                    return;
                }

                RaisePropertyChanging(() => ImapPort);
                if (value < 0) {
                    value = 0;
                }
                if (value > short.MaxValue) {
                    value = short.MaxValue;
                }
                _imapPort = value;
                RaisePropertyChanged(() => ImapPort);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        public string ImapUsername {
            get { return _imapUsername; }
            set {
                if (_imapUsername == value) {
                    return;
                }

                RaisePropertyChanging(() => ImapUsername);
                _imapUsername = value;
                RaisePropertyChanged(() => ImapUsername);
            }
        }

        public string SmtpPassword {
            get { return _smtpPassword.Caesar(ShiftOut); }
            set {
                if (_smtpPassword == value.Caesar(ShiftIn)) {
                    return;
                }

                RaisePropertyChanging(() => SmtpPassword);
                _smtpPassword = value.Caesar(ShiftIn);
                RaisePropertyChanged(() => SmtpPassword);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        public string SmtpUsername {
            get { return _smtpUsername; }
            set {
                if (_smtpUsername == value) {
                    return;
                }

                RaisePropertyChanging(() => SmtpUsername);
                _smtpUsername = value;
                RaisePropertyChanged(() => SmtpUsername);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        public string SmtpHost {
            get { return _smtpHost; }
            set {
                if (_smtpHost == value) {
                    return;
                }

                RaisePropertyChanging(() => SmtpHost);
                _smtpHost = value;
                RaisePropertyChanged(() => SmtpHost);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        public SecurityPolicy SmtpSecurity {
            get { return (SecurityPolicy)_smtpSecurity; }
            set {
                if (_smtpSecurity == (short)value) {
                    return;
                }

                RaisePropertyChanging(() => SmtpSecurity);
                _smtpSecurity = (short)value;
                RaisePropertyChanged(() => SmtpSecurity);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        public short SmtpPort {
            get { return _smtpPort; }
            set {
                if (_smtpPort == value) {
                    return;
                }

                RaisePropertyChanging(() => SmtpPort);
                if (value < 0) {
                    value = 0;
                }
                if (value > short.MaxValue) {
                    value = short.MaxValue;
                }

                _smtpPort = value;
                RaisePropertyChanged(() => SmtpPort);
            }
        }

        public bool IsTestSuccessful {
            get { return _isTestSuccessful; }
            set {
                if (_isTestSuccessful == value) {
                    return;
                }

                RaisePropertyChanging(() => IsTestSuccessful);
                _isTestSuccessful = value;
                RaisePropertyChanged(() => IsTestSuccessful);
            }
        }

        public bool IsTesting {
            get { return _isTesting; }
            set {
                if (_isTesting == value) {
                    return;
                }

                RaisePropertyChanging(() => IsTesting);
                RaisePropertyChanging(() => IsNotTesting);
                _isTesting = value;
                RaisePropertyChanged(() => IsTesting);
                RaisePropertyChanged(() => IsNotTesting);
            }
        }

        public bool IsNotTesting {
            get { return !IsTesting; }
        }

        public bool IsRequesting {
            get { return _isRequesting; }
            set {
                if (_isRequesting == value) {
                    return;
                }

                RaisePropertyChanging(() => IsRequesting);
                _isRequesting = value;
                RaisePropertyChanged(() => IsRequesting);
            }
        }

        private void Configure(clientConfig config) {
            if (!config.emailProvider.Any()) {
                return;
            }

            var provider = config.emailProvider.First();
            var imap = provider.incomingServer.FirstOrDefault(x => x.type.ContainsIgnoreCase("imap"));
            if (imap != null) {
                ImapHost = imap.hostname;
                ImapSecurity = imap.socketType.ToSecurityPolicy();
                ImapPort = short.Parse(imap.port);
                ImapUsername = GetImapUsernameFromMacro(imap);
            }

            var smtp = provider.outgoingServer.FirstOrDefault(x => x.type.ContainsIgnoreCase("smtp"));
            if (smtp == null)
                return;

            SmtpHost = smtp.hostname;
            SmtpSecurity = smtp.socketType.ToSecurityPolicy();
            SmtpPort = short.Parse(smtp.port);
            SmtpUsername = GetSmtpUsernameFromMacro(smtp);
        }

        private string GetImapUsernameFromMacro(clientConfigEmailProviderIncomingServer config) {
            return config.username == "%EMAILADDRESS%" ? EmailAddress : EmailAddress.Split('@').First();
        }

        private string GetSmtpUsernameFromMacro(clientConfigEmailProviderOutgoingServer config) {
            return config.username == "%EMAILADDRESS%" ? EmailAddress : EmailAddress.Split('@').First();
        }

        public async Task AutoConfigAsync() {
            var domain = EmailAddress.Split('@').Last();
            var url = string.Format("https://live.mozillamessaging.com/autoconfig/v1.1/{0}", domain);
            using (var client = new WebClient()) {
                try {
                    IsRequesting = true;
                    var stream = await client.OpenReadTaskAsync(new Uri(url, UriKind.Absolute));
                    var serializer = new XmlSerializer(typeof(clientConfig));
                    var config = serializer.Deserialize(stream) as clientConfig;
                    Configure(config);
                }
                catch (WebException) {
                    MakeEducatedGuess();
                }
                finally {
                    IsRequesting = false;
                }
            }
        }

        private void InvalidateCommands() {
            ConfigCommand.OnCanExecuteChanged(EventArgs.Empty);
            TestingCommand.OnCanExecuteChanged(EventArgs.Empty);
        }

        private void MakeEducatedGuess() {

        }

        public async Task TestConfiguration() {
            IsTesting = true;

            NetworkTest = new SettingsTestContext { Text = Resources.NetworkTestMessage };
            ImapConnectionTest = new SettingsTestContext { Text = Resources.ImapTestMessage };
            SmtpConnectionTest = new SettingsTestContext { Text = Resources.SmtpTestMessage };

            CheckConnectivity();
            await CheckImapConfigAsync();
            await CheckSmtpConfigAsync();

            CommitCommand.OnCanExecuteChanged(EventArgs.Empty);
            IsTestSuccessful = NetworkTest.IsSuccessful
                && ImapConnectionTest.IsSuccessful
                && SmtpConnectionTest.IsSuccessful;

            IsTesting = false;
        }

        private async Task CheckImapConfigAsync() {
            try {
                ImapConnectionTest.IsActive = true;
                using (var connection = new ImapConnection { Security = ImapSecurity }) {
                    using (var authenticator = await connection.ConnectAsync(ImapHost, ImapPort)) {
                        await authenticator.LoginAsync(ImapUsername, ImapPassword);
                        if (authenticator.IsAuthenticated) {
                            ImapConnectionTest.Text = Resources.ImapTestSuccessMessage;
                            ImapConnectionTest.IsSuccessful = true;
                        } else {
                            ImapConnectionTest.Error = new InvalidOperationException(Resources.AuthenticationFailedMessage);
                            ImapConnectionTest.Text = Resources.ImapTestFailureMessage;
                            ImapConnectionTest.IsSuccessful = false;
                        }
                    }
                }
            }
            catch (Exception ex) {
                ImapConnectionTest.Text = Resources.ImapTestFailureMessage;
                ImapConnectionTest.Error = ex;
            }
            finally {
                ImapConnectionTest.IsActive = false;
            }
        }

        private async Task CheckSmtpConfigAsync() {
            try {
                SmtpConnectionTest.IsActive = true;

                var resource =
                Application.GetResourceStream(new Uri("/Resources/smtp.message.html",
                                                      UriKind.Relative));
                Debug.Assert(resource != null);
                var body = await resource.Stream.ToUtf8StringAsync();

                using (var connection = new SmtpConnection { Security = SmtpSecurity }) {
                    using (var authenticator = await connection.ConnectAsync(SmtpHost, SmtpPort)) {
                        using (var session = await authenticator.LoginAsync(SmtpUsername, SmtpPassword)) {
                            var identity = IdentitySelectionSource.Current;

                            var subject = Resources.SmtpTestMessageSubject;
                            var from = new MailAddress(EmailAddress, "Paranoia");

                            var name = identity == null ? string.Empty : identity.Name;
                            var to = new MailAddress(EmailAddress, name);

                            using (var message = new MailMessage(from, to) {
                                HeadersEncoding = Encoding.UTF8,
                                BodyTransferEncoding = TransferEncoding.Base64,
                                SubjectEncoding = Encoding.UTF8,
                                BodyEncoding = Encoding.UTF8,
                                Subject = subject,
                                Body = body,
                                IsBodyHtml = true
                            }) {
                                await session.SendAsync(message);
                            }

                            SmtpConnectionTest.Text = Resources.SmtpTestSuccessMessage;
                            SmtpConnectionTest.IsSuccessful = true;
                        }
                    }
                }
            }
            catch (Exception ex) {
                SmtpConnectionTest.Text = Resources.SmtpTestFailureMessage;
                SmtpConnectionTest.Error = ex;
            }
            finally {
                SmtpConnectionTest.IsActive = false;
            }
        }

        private void CheckConnectivity() {
            try {
                NetworkTest.IsActive = true;
                NetworkTest.IsSuccessful = NetworkInterface.GetIsNetworkAvailable();
                NetworkTest.Text = NetworkTest.IsSuccessful ? Resources.NetworkTestSuccessMessage : Resources.NetworkTestFailureMessage;
            }
            catch (Exception ex) {
                NetworkTest.Text = Resources.ImapTestFailureMessage;
                NetworkTest.Error = ex;
            }
            finally {
                NetworkTest.IsActive = false;
            }
        }
    }
}