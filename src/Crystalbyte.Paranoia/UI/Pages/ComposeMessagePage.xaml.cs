#region Using directives

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Media;
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
            context.DocumentTextRequested += OnDocumentTextRequested;
            context.Finished += OnFinished;
            DataContext = context;
        }

        private void OnFinished(object sender, EventArgs e) {
            var window = GetParentWindow(this);
            if (window != null) {
                window.Close();
            }
        }

        private void OnDocumentTextRequested(object sender, DocumentTextRequestedEventArgs e) {
            var html = HtmlControl.GetEditorDocument();
            e.Document = html;
        }

        private async void Reset() {
            var composition = (MailCompositionContext)DataContext;
            await composition.ResetAsync();
        }

        public MailCompositionContext Composition {
            get { return (MailCompositionContext)DataContext; }
        }

        private async void OnRecipientsBoxItemsSourceRequested(object sender, ItemsSourceRequestedEventArgs e) {
            await Composition.QueryRecipientsAsync(e.Text);
        }

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


        private async void PrepareAsReply(IDictionary<string, string> arguments) {
            MailMessageReader replyMessage;
            var temp = Int64.Parse(arguments["id"]);

            using (var database = new DatabaseContext()) {
                var message = await database.MimeMessages
                    .Where(x => x.MessageId == temp)
                    .ToArrayAsync();

                if (!message.Any())
                    throw new Exception("Message has not yet been loaded, menu must be disabled until it is.");

                replyMessage = new MailMessageReader(message[0].Data);
            }
            var context = (MailCompositionContext)DataContext;
            context.Subject = "RE: " + replyMessage.Headers.Subject;
            context.Source = string.Format("asset://paranoia/message/reply?id={0}", temp);
            //TODO add recipient
        }

        #region Implementation of INavigationAware

        public void OnNavigated(NavigationEventArgs e) {
            Reset();

            var arguments = e.Uri.OriginalString.ToPageArguments();
            if (arguments.ContainsKey("action") && arguments["action"] == "reply") {
                PrepareAsReply(arguments);
                return;
            }

            if (arguments.ContainsKey("action") && arguments["action"] == "new") {
                PrepareAsNew();
            }
        }

        private void PrepareAsNew() {
            var context = (MailCompositionContext)DataContext;
            context.Source = "asset://paranoia/message/new";
        }

        #endregion

        private void DropHtmlControl(object sender, DragEventArgs e) {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var context = DataContext as MailCompositionContext;
            if (files == null | context == null)
                return;

            files.ToList().ForEach(x => context.Attachments.Add(new AttachmentContext(context, x)));
        }

        public static Window GetParentWindow(DependencyObject child) {
            var parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) {
                return null;
            }

            var parent = parentObject as Window;
            return parent ?? GetParentWindow(parentObject);
        }
    }
}