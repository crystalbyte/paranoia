using System.Windows.Input;
using System.Windows.Media;

namespace Crystalbyte.Paranoia.Commands {
    public interface IAppBarCommand : ICommand {
        string Text { get; }
        string Category { get; }
        ImageSource Image { get; }
        int Position { get; }
    }
}
