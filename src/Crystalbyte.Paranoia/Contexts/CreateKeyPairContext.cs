#region Using directives

using System;
using System.IO;
using System.Windows.Input;
using Crystalbyte.Paranoia.Cryptography;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.UI.Commands;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    public sealed class CreateKeyPairContext : NotificationObject {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ICommand _createKeyPairCommand;
        private readonly string _benFranklinQuote;
        private readonly string _benFranklin;


        public CreateKeyPairContext() {
            _createKeyPairCommand = new RelayCommand(OnCreateKeyPair);
            _benFranklinQuote =
                "“Those who surrender freedom for security will not have, nor do they deserve, either one.”";
            _benFranklin = "Benjamin Franklin (1706-1790)";
        }

        public ICommand CreateKeyPairCommand {
            get { return _createKeyPairCommand; }
        }

        private static async void OnCreateKeyPair(object obj) {
            try {
                App.Context.ClosePopup();

                using (var crypto = new PublicKeyCrypto()) {
                    crypto.Init();
                    var dir = AppContext.GetKeyDirectory().FullName;

                    var publicKey = Path.Combine(dir, Settings.Default.PublicKeyFile);
                    await crypto.SavePublicKeyAsync(publicKey);

                    var privateKey = Path.Combine(dir, Settings.Default.PrivateKeyFile);
                    await crypto.SavePrivateKeyAsync(privateKey);

                    await App.Context.InitKeysAsync();
                    await App.Context.RunAsync();
                }
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public string BenFranklinQuote {
            get { return _benFranklinQuote; }
        }

        public string BenFranklin {
            get { return _benFranklin; }
        }
    }
}