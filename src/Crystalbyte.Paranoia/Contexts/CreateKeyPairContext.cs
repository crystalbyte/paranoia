using System.Windows.Input;
using Crystalbyte.Paranoia.UI.Commands;

namespace Crystalbyte.Paranoia {
    public sealed class CreateKeyPairContext : NotificationObject {

        private readonly ICommand _createKeyPairCommand;
        private readonly string _benFranklinQuote;
        private readonly string _benFranklin;

        public CreateKeyPairContext() {
            _createKeyPairCommand = new RelayCommand(CreateKeyPair);
            _benFranklinQuote = "“Those who surrender freedom for security will not have, nor do they deserve, either one.”";
            _benFranklin = "Benjamin Franklin (1706-1790)";
        }

        public ICommand CreateKeyPairCommand {
            get { return _createKeyPairCommand; }
        }

        private static async void CreateKeyPair(object obj) {
            App.Context.ClosePopup();
            await App.Context.RunAsync();
        }

        public string BenFranklinQuote {
            get { return _benFranklinQuote; }
        }

        public string BenFranklin {
            get { return _benFranklin; }
        }
    }
}
