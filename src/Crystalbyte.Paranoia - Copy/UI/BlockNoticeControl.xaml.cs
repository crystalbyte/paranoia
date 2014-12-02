using System.ComponentModel;
namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for BlockNoticeControl.xaml
    /// </summary>
    public partial class BlockNoticeControl {
        public BlockNoticeControl() {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this)) {
                return;
            }

            DataContext = App.Context.SelectedMessage;
            App.Context.MessageSelectionChanged 
                += (sender, e) => DataContext = App.Context.SelectedMessage;
        }
    }
}
