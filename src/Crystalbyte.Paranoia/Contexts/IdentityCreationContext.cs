#region Using directives

using System.ComponentModel.DataAnnotations;
using System.Composition;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Windows.Input;
using Crystalbyte.Paranoia.Commands;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.Models;
using System;
using System.Security;
using System.Windows.Navigation;
using Crystalbyte.Paranoia.Messaging;
using System.Windows.Controls;
using Crystalbyte.Paranoia.UI;
using System.Threading.Tasks;
using System.Net;
using System.Xml.Serialization;

#endregion

namespace Crystalbyte.Paranoia.Contexts {
    public sealed class IdentityCreationContext : ValidationObject<IdentityCreationContext> {

        #region Private Fields

        private bool _isActive;
        private string _name;
        private short _imapPort;
        private short _smtpPort;
        private string _address;
        private string _imapHost;
        private string _smtpHost;
        private string _gravatarUrl;
        private string _imapPassword;
        private string _smtpPassword;
        private string _imapUsername;
        private string _smtpUsername;
        private static string _password;
        private ConfigState _configState;
        private SecurityPolicy _imapSecurity;
        private SecurityPolicy _smtpSecurity;
        private string _passwordConfirmation;

        #endregion

        #region Construction

        public IdentityCreationContext() {
            ConfigCommand = new RelayCommand(OnCanConfigure, OnConfigure);
            CancelCommand = new RelayCommand(OnCancel);
            GoBackCommand = new RelayCommand(OnCanGoBack, OnGoBack);
        }

        private bool OnCanGoBack(object arg) {
            if (arg == null) {
                return true;
            }
            var page = (Page)arg;
            return page.NavigationService.CanGoBack;
        }

        private void OnGoBack(object obj) {
            var page = (Page)obj;
            page.NavigationService.GoBack();
        }

        #endregion

        private void ClearPassword() {
            _password = string.Empty;
            _passwordConfirmation = string.Empty;
        }

        public static string GetPassword() {
            return _password;
        }

        public event EventHandler Finished;

        private void OnFinished() {
            var handler = Finished;
            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
        }

        #region Overrides for ValidationObject<T>

        protected override void OnValidated(EventArgs e) {
            base.OnValidated(e);
            ConfigCommand.Refresh();
        }

        #endregion

        public RelayCommand ConfigCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand GoBackCommand { get; set; }
        public ConfigState ConfigState {
            get { return _configState; }
            set {
                if (_configState == value) {
                    return;
                }

                RaisePropertyChanging(() => ConfigState);
                _configState = value;
                RaisePropertyChanged(() => ConfigState);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NameRequiredErrorText")]
        [StringLength(64, ErrorMessageResourceType = typeof(Resources),
            ErrorMessageResourceName = "MaxStringLength64ErrorText")]
        public string Name {
            get { return _name; }
            set {
                if (_name == value) {
                    return;
                }

                RaisePropertyChanging(() => Name);
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        [RegularExpression(RegexPatterns.Email, ErrorMessageResourceType = typeof(Resources),
            ErrorMessageResourceName = "InvalidEmailFormatErrorText")]
        public string Address {
            get { return _address; }
            set {
                if (_address == value) {
                    return;
                }

                RaisePropertyChanging(() => Address);
                _address = value;
                RaisePropertyChanged(() => Address);
                OnAddressChanged();
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

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        public string ImapPassword {
            get { return _imapPassword; }
            set {
                if (_imapPassword == value) {
                    return;
                }

                RaisePropertyChanging(() => ImapPassword);
                _imapPassword = value;
                RaisePropertyChanged(() => ImapPassword);
            }
        }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "NullOrEmptyErrorText")]
        public string SmtpPassword {
            get { return _smtpPassword; }
            set {
                if (_smtpPassword == value) {
                    return;
                }

                RaisePropertyChanging(() => SmtpPassword);
                _smtpPassword = value;
                RaisePropertyChanged(() => SmtpPassword);
            }
        }

        public SecurityPolicy ImapSecurity {
            get { return _imapSecurity; }
            set {
                if (_imapSecurity == value) {
                    return;
                }

                RaisePropertyChanging(() => ImapSecurity);
                _imapSecurity = value;
                RaisePropertyChanged(() => ImapSecurity);
            }
        }

        public SecurityPolicy SmtpSecurity {
            get { return _smtpSecurity; }
            set {
                if (_smtpSecurity == value) {
                    return;
                }

                RaisePropertyChanging(() => SmtpSecurity);
                _smtpSecurity = value;
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
                _smtpPort = value;
                RaisePropertyChanged(() => SmtpPort);
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
        public short ImapPort {
            get { return _imapPort; }
            set {
                if (_imapPort == value) {
                    return;
                }

                RaisePropertyChanging(() => ImapPort);
                _imapPort = value;
                RaisePropertyChanged(() => ImapPort);
            }
        }

        [PasswordPolicy(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "PasswordComplexityInsufficientErrorText")]
        public string Password {
            get { return _password; }
            set {
                if (_password == value) {
                    return;
                }

                RaisePropertyChanging(() => Password);
                _password = value;
                RaisePropertyChanged(() => Password);
            }
        }

        [PasswordMatch(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "PasswordNotMatchingErrorText")]
        public string PasswordConfirmation {
            get { return _passwordConfirmation; }
            set {
                if (_passwordConfirmation == value) {
                    return;
                }

                RaisePropertyChanging(() => PasswordConfirmation);
                _passwordConfirmation = value;
                RaisePropertyChanged(() => PasswordConfirmation);
            }
        }

        public string GravatarUrl {
            get { return _gravatarUrl; }
            set {
                if (_gravatarUrl == value) {
                    return;
                }

                RaisePropertyChanging(() => GravatarUrl);
                _gravatarUrl = value;
                RaisePropertyChanged(() => GravatarUrl);
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
            }
        }

        private void OnAddressChanged() {
            CreateGravatarUrl();
        }

        private void OnCancel(object obj) {
            ClearPassword();
            OnFinished();

            var page = obj as Page;
            if (page != null) {
                var service = page.NavigationService;
                while (service.CanGoBack) {
                    service.GoBack();
                }
            }
        }

        private bool OnCanConfigure(object parameter) {
            return ValidFor(() => Address)
                && ValidFor(() => Name)
                && ValidFor(() => ImapPassword);
        }

        private async void OnConfigure(object parameter) {
            var password = ImapPassword;

            var uri = string.Format("/UI/{0}.xaml", typeof(ServerConfigPage).Name);
            var page = (Page)parameter;
            page.NavigationService.Navigate(new Uri(uri, UriKind.Relative));

            await AutoConfigAsync();

            ImapPassword = password;
            SmtpPassword = password;
        }

        private void Resolve(clientConfig config) {
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
            return config.username == "%EMAILADDRESS%" ? Address : Address.Split('@').First();
        }

        private string GetSmtpUsernameFromMacro(clientConfigEmailProviderOutgoingServer config) {
            return config.username == "%EMAILADDRESS%" ? Address : Address.Split('@').First();
        }

        public async Task AutoConfigAsync() {
            var domain = Address.Split('@').Last();
            var url = string.Format("https://live.mozillamessaging.com/autoconfig/v1.1/{0}", domain);
            using (var client = new WebClient()) {
                try {
                    ConfigState = ConfigState.Active;
                    var stream = await client.OpenReadTaskAsync(new Uri(url, UriKind.Absolute));
                    var serializer = new XmlSerializer(typeof(clientConfig));
                    var config = serializer.Deserialize(stream) as clientConfig;
                    Resolve(config);
                    ConfigState = ConfigState.Succeeded;
                } catch (WebException) {
                    MakeEducatedGuess(domain);
                    ConfigState = ConfigState.Failed;
                } 
            }
        }

        private void MakeEducatedGuess(string domain) {
            ImapUsername = Address;
            SmtpUsername = Address;                                       
        }

        public void CreateGravatarUrl() {
            using (var md5 = MD5.Create()) {
                var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(Address.Trim()));
                using (var writer = new StringWriter()) {
                    foreach (var b in bytes) {
                        writer.Write(b.ToString("x2"));
                    }
                    GravatarUrl = string.Format("http://www.gravatar.com/avatar/{0}?s=200&d=mm", writer);
                }
            }
        }

        private sealed class PasswordMatchAttribute : ValidationAttribute {
            public override bool IsValid(object value) {
                var password = value as string;
                if (string.IsNullOrWhiteSpace(password)) {
                    return false;
                }
                return password == IdentityCreationContext.GetPassword();
            }
        }
    }
}