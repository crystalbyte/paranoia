using System.Windows.Input;
using System.Windows.Media;

namespace Crystalbyte.Paranoia {
    public interface IQuickAccessConform {
        object ToolTip { get; }
        ImageSource QuickAccessImageSource { get; }
        ICommand Command { get; }
        object CommandParameter { get; }
    }
}
