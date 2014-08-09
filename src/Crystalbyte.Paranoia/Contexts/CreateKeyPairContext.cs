using System.Windows.Input;
using Crystalbyte.Paranoia.UI.Commands;

namespace Crystalbyte.Paranoia {
    public sealed class CreateKeyPairContext : NotificationObject {

        private readonly ICommand _createKeyPairCommand;

        public CreateKeyPairContext() {
            _createKeyPairCommand = new RelayCommand(CreateKeyPair);
        }

        public ICommand CreateKeyPairCommand {
            get { return _createKeyPairCommand; }
        }

        private static async void CreateKeyPair(object obj) {
            App.Context.ClosePopup();
            await App.Context.RunAsync();
        }
    }
}
