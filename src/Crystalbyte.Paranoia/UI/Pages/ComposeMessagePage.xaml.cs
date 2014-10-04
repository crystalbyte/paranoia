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

        private async void OnRecipientsBoxItemsSourceRequested(object sender, ItemsSourceRequestedEventArgs e) {
            var composition = (MailCompositionContext)DataContext;
            await composition.QueryRecipientsAsync(e.Text);
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

        private void PrepareAsNew() {
            var context = (MailCompositionContext)DataContext;
            context.Source = "asset://paranoia/message/new";
        }

        private async void PrepareAsReply(IDictionary<string, string> arguments) {
            MailContactContext from;
            MailMessageReader replyMessage;
            var id = Int64.Parse(arguments["id"]);

            using (var database = new DatabaseContext()) {
                var message = await database.MimeMessages
                    .Where(x => x.MessageId == id)
                    .ToArrayAsync();

                if (!message.Any())
                    throw new InvalidOperationException("Message has not yet been loaded, menu must be disabled until it is.");

                replyMessage = new MailMessageReader(message[0].Data);
                from = new MailContactContext(await database.MailContacts
                    .FirstAsync(x => x.Address == replyMessage.Headers.From.Address));
            }

            var context = (MailCompositionContext)DataContext;
            context.Subject = "RE: " + replyMessage.Headers.Subject;
            context.Source = string.Format("asset://paranoia/message/reply?id={0}", id);

            RecipientsBox.Preset(new[] { from });
        }

        #region Implementation of INavigationAware

        public void OnNavigated(NavigationEventArgs e) {
            Reset();

            var arguments = e.Uri.OriginalString.ToPageArguments();
            if (arguments.ContainsKey("action") && arguments["action"] == "new") {
                PrepareAsNew();
                return;
            }
            
            if (arguments.ContainsKey("action") && arguments["action"] == "reply") {
                PrepareAsReply(arguments);
                return;
            }

            if (arguments.ContainsKey("action") && arguments["action"] == "reply-all") {
                PrepareAsReplyAll(arguments);
                return;
            }

            if (arguments.ContainsKey("action") && arguments["action"] == "forward") {
                PrepareAsForward(arguments);
                return;
            }
        }

        private void PrepareAsReplyAll(Dictionary<string, string> arguments) {
            throw new NotImplementedException();
        }

        private void PrepareAsForward(Dictionary<string, string> arguments) {
            throw new NotImplementedException();
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