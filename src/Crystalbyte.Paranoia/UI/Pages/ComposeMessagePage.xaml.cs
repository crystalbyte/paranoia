using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Navigation;
using System.Data.Entity;
using System.Linq;

namespace Crystalbyte.Paranoia.UI.Pages
{
    /// <summary>
    /// Interaction logic for WriteMessagePage.xaml
    /// </summary>
    public partial class ComposeMessagePage : INavigationAware
    {

        public ComposeMessagePage()
        {
            var context = new MailCompositionContext();
            context.Finished += OnShutdownRequested;
            context.DocumentTextRequested += OnDocumentTextRequested;
            DataContext = context;
            InitializeComponent();

            var window = (MainWindow)Application.Current.MainWindow;
            window.OverlayChanged += OnOverlayChanged;
        }

        private static void OnShutdownRequested(object sender, EventArgs e)
        {
            App.Context.CloseOverlay();
        }

        private void OnDocumentTextRequested(object sender, DocumentTextRequestedEventArgs e)
        {
            e.Document = HtmlControl.GetDocument();
        }

        private async void Reset()
        {
            var composition = (MailCompositionContext)DataContext;
            await composition.ResetAsync();
        }

        private void OnOverlayChanged(object sender, EventArgs e)
        {
            var window = (MainWindow)Application.Current.MainWindow;
            if (!window.IsOverlayVisible)
            {
                RecipientsBox.Close();
            }
        }

        public MailCompositionContext Composition
        {
            get { return (MailCompositionContext)DataContext; }
        }

        private async void OnRecipientsBoxItemsSourceRequested(object sender, ItemsSourceRequestedEventArgs e)
        {
            await Composition.QueryRecipientsAsync(e.Text);
        }

        #region Implementation of INavigationAware

        public void OnNavigated(NavigationEventArgs e)
        {
            Reset();
            if (!String.IsNullOrEmpty(e.Uri.OriginalString))
            {
                PrepareAsReply(e.Uri.OriginalString);
            }
        }

        #endregion

        private void OnRecipientsBoxSelectionChanged(object sender, EventArgs e)
        {
            var addresses = RecipientsBox
                .SelectedValues
                .Select(x => x is MailContactContext
                    ? ((MailContactContext)x).Address
                    : x as string);

            var context = (MailCompositionContext)DataContext;
            context.Recipients.Clear();
            context.Recipients.AddRange(addresses);
        }


        private static async void PrepareAsReply(string s)
        {
            Data.MimeMessageModel[] message;
            Regex ItemRegex = new Regex(@"[0-9]+", RegexOptions.Compiled);

            using (var database = new Crystalbyte.Paranoia.Data.DatabaseContext())
            {
                Int64 temp = Int64.Parse(ItemRegex.Match(s).Value);
                message = await database.MimeMessages
                .Where(x => x.MessageId == temp)
                .ToArrayAsync();
            }
        }
    }
}
