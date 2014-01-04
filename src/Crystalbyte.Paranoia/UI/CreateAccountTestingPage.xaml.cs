using System.Windows;
using Crystalbyte.Paranoia.Contexts;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for CreateAccountTestingPage.xaml
    /// </summary>
    public partial class CreateAccountTestingPage {

        public CreateAccountTestingPage() {
            ScreenContext = App.AppContext.CreateAccountScreenContext;
            Loaded += OnPageLoaded;
            InitializeComponent();
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e) {
            await ScreenContext.TestConfiguration();
        }

        public CreateAccountScreenContext ScreenContext {
            get { return DataContext as CreateAccountScreenContext; }
            set { DataContext = value; }
        }
    }
}
