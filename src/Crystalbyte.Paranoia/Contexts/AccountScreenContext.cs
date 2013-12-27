using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;
using Crystalbyte.Paranoia.Commands;
using System.ComponentModel.DataAnnotations;
using Crystalbyte.Paranoia.Messaging;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.Models;

namespace Crystalbyte.Paranoia.Contexts {

    [Export, Shared]
    public sealed class AccountScreenContext : ValidationObject<AccountScreenContext> {

        #region Private Fields

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
        private bool _isChecking;
        private short _smtpPort;
        private string _emailAddress;

        private static readonly IEnumerable<SecurityPolicy> Policies;

        #endregion

        #region Construction

        public AccountScreenContext() {
            CancelCommand = new RelayCommand(OnCancel);
            ContinueCommand = new RelayCommand(OnCanContinue, OnContinue);
            ResolveCommand = new RelayCommand(OnCanResolve, OnResolve);
        }

        static AccountScreenContext() {
            Policies = new List<SecurityPolicy> { SecurityPolicy.None, SecurityPolicy.Implicit, SecurityPolicy.Explicit };
        }

        #endregion

        private async void OnResolve(object obj) {
            await AutoConfigAsync();
        }

        private bool OnCanResolve(object arg) {
            return !string.IsNullOrWhiteSpace(EmailAddress)
                && !string.IsNullOrWhiteSpace(ImapPassword);
        }

        private bool OnCanContinue(object arg) {
            return IsValid;
        }

        private void OnCancel(object obj) {
            IsActive = false;
        }

        private void OnContinue(object obj) {
            
        }

        #region ValidationObject Overrides

        protected override void OnValidated(EventArgs e) {
            base.OnValidated(e);
            ContinueCommand.OnCanExecuteChanged(EventArgs.Empty);
        }

        #endregion

        #region Import Declarations

        [Import]
        public AppContext AppContext { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied() {
            // Nada    
        }

        #endregion

        public ICommand CancelCommand { get; private set; }
        public RelayCommand ContinueCommand { get; private set; }
        public RelayCommand ResolveCommand { get; private set; }
        public IEnumerable<SecurityPolicy> PolicySource { get { return Policies; } }

        public event EventHandler Activated;

        public void OnActivated(EventArgs e) {
            var handler = Activated;
            if (handler != null) 
                handler(this, e);
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
                ResolveCommand.OnCanExecuteChanged(EventArgs.Empty);
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

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "PasswordRequiredErrorText")]
        public string ImapPassword {
            get { return _imapPassword; }
            set {
                if (_imapPassword == value) {
                    return;
                }

                RaisePropertyChanging(() => ImapPassword);
                _imapPassword = value;
                RaisePropertyChanged(() => ImapPassword);
                ResolveCommand.OnCanExecuteChanged(EventArgs.Empty);
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

        public bool IsChecking {
            get { return _isChecking; }
            set {
                if (_isChecking == value) {
                    return;
                }

                RaisePropertyChanging(() => IsChecking);
                _isChecking = value;
                RaisePropertyChanged(() => IsChecking);
            }
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
            SmtpPassword = ImapPassword;
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
                    IsChecking = true;
                    var stream = await client.OpenReadTaskAsync(new Uri(url, UriKind.Absolute));
                    var serializer = new XmlSerializer(typeof(clientConfig));
                    var config = serializer.Deserialize(stream) as clientConfig;
                    Resolve(config);
                }
                catch (WebException) {
                    MakeEducatedGuess();
                }
                finally {
                    IsChecking = false;
                }
            }
        }

        private void MakeEducatedGuess() {
            
        }
    }
}
