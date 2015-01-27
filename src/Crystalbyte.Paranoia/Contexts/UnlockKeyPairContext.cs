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
    public sealed class UnlockKeyPairContext : NotificationObject {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ICommand _unlockKeyPairCommand;

        public UnlockKeyPairContext() {
            _unlockKeyPairCommand = new RelayCommand(OnUnlockKeyPair);
        }

        public ICommand UnlockKeyPairCommand {
            get { return _unlockKeyPairCommand; }
        }

        private static void OnUnlockKeyPair(object obj) {
            try {
                App.Context.ClosePopup();

                //using (var crypto = new PublicKeyCrypto()) {
                //    crypto.Init();
                //    var dir = AppContext.GetKeyDirectory().FullName;

                //    var publicKey = Path.Combine(dir, Settings.Default.PublicKeyFile);
                //    await crypto.SavePublicKeyAsync(publicKey);

                //    var privateKey = Path.Combine(dir, Settings.Default.PrivateKeyFile);
                //    await crypto.SavePrivateKeyAsync(privateKey);

                //    await App.Context.InitKeysAsync();
                //    await App.Context.RunAsync();
                //}
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }
    }
}