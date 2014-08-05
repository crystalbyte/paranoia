using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Navigation;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;
using Crystalbyte.Paranoia.Mail;
using System.Text;

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
            var arguments = GetArguments(e.Uri.OriginalString);
            if (arguments.ContainsValue("reply"))
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


        private async void PrepareAsReply(string s)
        {
            Data.MimeMessageModel[] message;
            var ItemRegex = new Regex(@"[0-9]+", RegexOptions.Compiled);
            MailMessage replyMessage;

            using (var database = new Crystalbyte.Paranoia.Data.DatabaseContext())
            {
                Int64 temp = Int64.Parse(ItemRegex.Match(s).Value);
                message = await database.MimeMessages
                .Where(x => x.MessageId == temp)
                .ToArrayAsync();
                replyMessage = new MailMessage(Encoding.UTF8.GetBytes(message[0].Data));
            }
            var context = (MailCompositionContext)DataContext;
            context.Subject = "RE: "+replyMessage.Headers.Subject;
        }

        private static Dictionary<String, String> GetArguments(String s)
        {
            Dictionary<String, String> dic = new Dictionary<string, string>();
            var pattern = "[A-Za-z0-9]+=[A-Za-z0-9]+";
            var matches = Regex.Matches(s, pattern,
            RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var temp = match.Value.ToString().Split('=');
                dic.Add(temp[0], temp[1]);
            }
            return dic;
        }
    }
}
