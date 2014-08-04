using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Navigation;

namespace Crystalbyte.Paranoia.UI.Pages {
    /// <summary>
    /// Interaction logic for WriteMessagePage.xaml
    /// </summary>
    public partial class ComposeMessagePage : INavigationAware {

        public ComposeMessagePage() {
            var context = new MailCompositionContext();
            context.Finished += OnShutdownRequested;
            context.DocumentTextRequested += OnDocumentTextRequested;
            DataContext = context;
            InitializeComponent();

            var window = (MainWindow)Application.Current.MainWindow;
            window.OverlayChanged += OnOverlayChanged;
        }

        private static void OnShutdownRequested(object sender, EventArgs e) {
            App.Context.CloseOverlay();
        }

        private void OnDocumentTextRequested(object sender, DocumentTextRequestedEventArgs e) {
            e.Document = HtmlControl.GetDocument();
        }

        private async void Reset() {
            var composition = (MailCompositionContext)DataContext;
            await composition.ResetAsync();
        }

        private void OnOverlayChanged(object sender, EventArgs e) {
            var window = (MainWindow)Application.Current.MainWindow;
            if (!window.IsOverlayVisible) {
                RecipientsBox.Close();
            }
        }

        public MailCompositionContext Composition {
            get { return (MailCompositionContext)DataContext; }
        }

        private async void OnRecipientsBoxItemsSourceRequested(object sender, ItemsSourceRequestedEventArgs e) {
            await Composition.QueryRecipientsAsync(e.Text);
        }

        #region Implementation of INavigationAware

        public void OnNavigated(NavigationEventArgs e) {
            Reset();

            if (!String.IsNullOrEmpty(e.Uri.OriginalString))
            {
                 Regex ItemRegex = new Regex(@"[0-9]+", RegexOptions.Compiled);
                foreach (Match ItemMatch in ItemRegex.Matches(e.Uri.OriginalString))
                {
                    Console.WriteLine(ItemMatch);
                }
            }
           

           
        }

        #endregion

        private void OnRecipientsBoxSelectionChanged(object sender, EventArgs e) {
            var addresses = RecipientsBox
                .SelectedValues
                .Select(x => x is MailContactContext
                    ? ((MailContactContext)x).Address
                    : x as string);

            var context = (MailCompositionContext)DataContext;
            context.Recipients.Clear();
            context.Recipients.AddRange(addresses);
        } 
    }
}
