using System;
using System.Windows;
using System.Windows.Controls;

namespace Crystalbyte.Paranoia.UI.Pages {
    /// <summary>
    /// Interaction logic for WriteMessagePage.xaml
    /// </summary>
    public partial class ComposeMessagePage {

        public ComposeMessagePage() {
            DataContext = new MailCompositionContext();
            InitializeComponent();
            Loaded += OnLoaded;

            var window = (MainWindow)Application.Current.MainWindow;
            window.OverlayChanged += OnOverlayChanged;
        }

        private void OnOverlayChanged(object sender, EventArgs e) {
            var window = (MainWindow)Application.Current.MainWindow;
            if (!window.IsOverlayVisible) {
                SuggestionBox.Close();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            SubjectTextBox.Focus();
        }

        public MailCompositionContext Composition {
            get { return (MailCompositionContext) DataContext; }
        }

        private async void OnAutoCompleteBoxOnItemsSourceRequested(object sender, ItemsSourceRequestedEventArgs e) {
            await Composition.QueryRecipientsAsync(e.Text);
        }
    }
}
