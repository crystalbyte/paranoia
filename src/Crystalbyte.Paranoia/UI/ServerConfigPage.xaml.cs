using System.ComponentModel;
using System.Windows;
using Crystalbyte.Paranoia.Contexts;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for CreateAccountTestingPage.xaml
    /// </summary>
    public partial class ServerConfigPage {

        public ServerConfigPage() {
            if (!DesignerProperties.GetIsInDesignMode(this)) {
                //ScreenContext = App.AppContext.CreateAccountScreenContext;
                Loaded += OnPageLoaded;
            }
            InitializeComponent();
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e) {
            //await ScreenContext.TestConfiguration();
        }

        private void OnPortPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e) {
            if (!char.IsDigit(e.Text, e.Text.Length - 1)) {
                e.Handled = true;
            }
        }

        //public CreateAccountScreenContext ScreenContext {
        //    get { return DataContext as CreateAccountScreenContext; }
        //    set { DataContext = value; }
        //}
    }
}
