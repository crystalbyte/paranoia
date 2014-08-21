﻿#region Using directives

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
        private readonly ICommand _createKeyPairCommand;
        private readonly string _benFranklinQuote;
        private readonly string _benFranklin;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CreateKeyPairContext() {
            _createKeyPairCommand = new RelayCommand(CreateKeyPair);
            _benFranklinQuote =
                "“Those who surrender freedom for security will not have, nor do they deserve, either one.”";
            _benFranklin = "Benjamin Franklin (1706-1790)";
        }

        public ICommand CreateKeyPairCommand {
            get { return _createKeyPairCommand; }
        }

        private static async void CreateKeyPair(object obj) {
            try {
                using (var crypto = new PublicKeyCrypto()) {
                    crypto.Init();
                    var dir = AppContext.GetKeyDirectory().FullName;

                    var publicKey = Path.Combine(dir, Settings.Default.PublicKeyFile);
                    await crypto.SavePublicKeyAsync(publicKey);

                    var privateKey = Path.Combine(dir, Settings.Default.PrivateKeyFile);
                    await crypto.SavePrivateKeyAsync(privateKey);

                    App.Context.OpenDecryptKeyPairDialog();
                    await App.Context.RunAsync();
                }
            }
            catch (Exception ex) {
                Logger.Error(ex.ToString());
            }
            finally {
                App.Context.ClosePopup();
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