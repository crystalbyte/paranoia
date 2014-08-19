#region Using directives

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Navigation;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;

#endregion

namespace Crystalbyte.Paranoia.UI.Pages {
    /// <summary>
    ///     Interaction logic for WriteMessagePage.xaml
    /// </summary>
    public partial class ComposeMessagePage : INavigationAware {
        public ComposeMessagePage() {
            InitializeComponent();

            var context = new MailCompositionContext();
            context.Finished += OnShutdownRequested;
            context.DocumentTextRequested += OnDocumentTextRequested;
            DataContext = context;

            var window = (MainWindow) Application.Current.MainWindow;
            window.FlyOutVisibilityChanged += OnFlyOutVisibilityChanged;
        }

        private static void OnShutdownRequested(object sender, EventArgs e) {
            App.Context.CloseFlyOut();
        }

        private void OnDocumentTextRequested(object sender, DocumentTextRequestedEventArgs e) {
            e.Document = HtmlControl.GetDocument();
        }

        private async void Reset() {
            var composition = (MailCompositionContext) DataContext;
            await composition.ResetAsync();
        }

        private void OnFlyOutVisibilityChanged(object sender, EventArgs e) {
            var window = (MainWindow) Application.Current.MainWindow;
            if (!window.IsFlyOutVisible) {
                RecipientsBox.Close();
            }
        }

        public MailCompositionContext Composition {
            get { return (MailCompositionContext) DataContext; }
        }

        private async void OnRecipientsBoxItemsSourceRequested(object sender, ItemsSourceRequestedEventArgs e) {
            await Composition.QueryRecipientsAsync(e.Text);
        }

        private void OnRecipientsBoxSelectionChanged(object sender, EventArgs e) {
            var addresses = RecipientsBox
                .SelectedValues
                .Select(x => x is MailContactContext
                    ? ((MailContactContext) x).Address
                    : x as string);

            var context = (MailCompositionContext) DataContext;
            context.Recipients.Clear();
            context.Recipients.AddRange(addresses);
        }


        private async void PrepareAsReply(IDictionary<string, string> arguments) {
            MailMessage replyMessage;
            using (var database = new DatabaseContext()) {
                var temp = Int64.Parse(arguments["id"]);
                var message = await database.MimeMessages
                    .Where(x => x.MessageId == temp)
                    .ToArrayAsync();

                replyMessage = new MailMessage(Encoding.UTF8.GetBytes(message[0].Data));
            }
            var context = (MailCompositionContext) DataContext;
            context.Subject = "RE: " + replyMessage.Headers.Subject;
        }

        #region Implementation of INavigationAware

        public void OnNavigated(NavigationEventArgs e) {
            Reset();

            var arguments = e.Uri.OriginalString.ToPageArguments();
            if (arguments.ContainsKey("action") && arguments["action"] == "reply") {
                PrepareAsReply(arguments);
            }
        }

        #endregion
    }
}