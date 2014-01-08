#region Using directives

using Crystalbyte.Paranoia.Contexts;
using System.ComponentModel;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///   Interaction logic for CreateAccountScreen.xaml
    /// </summary>
    public partial class CreateAccountScreen {
        public CreateAccountScreen() {
            if (!DesignerProperties.GetIsInDesignMode(this)) {
                ScreenContext = App.AppContext.CreateAccountScreenContext;    
            }
            InitializeComponent();
        }

        public CreateAccountScreenContext ScreenContext {
            get { return DataContext as CreateAccountScreenContext; }
            set { DataContext = value; }
        }
    }
}