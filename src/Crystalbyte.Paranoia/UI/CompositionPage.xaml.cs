#region Using directives

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for ComposeMessagePage.xaml
    /// </summary>
    public partial class CompositionPage : INavigationAware {

        public CompositionPage() {
            InitializeComponent();

            var context = new MailCompositionContext();
            context.DocumentTextRequested += OnDocumentTextRequested;
            context.Finished += OnFinished;

            HtmlControl.DocumentReady += OnDocumentReady;
            DataContext = context;
        }

        private async void OnDocumentReady(object sender, EventArgs e) {
            await ChangeSignatureAsync();
        }

        private void OnFinished(object sender, EventArgs e) {
            var window = GetParentWindow(this);
            if (window != null) {
                window.Close();
            }
        }

        private void OnDocumentTextRequested(object sender, DocumentTextRequestedEventArgs e) {
            var html = HtmlControl.GetComposition();
            e.Document = html;
        }

        private async void Reset() {
            var composition = (MailCompositionContext)DataContext;
            await composition.ResetAsync();
        }

        private async void OnRecipientsBoxItemsSourceRequested(object sender, ItemsSourceRequestedEventArgs e) {
            RecipientsBox.ItemsSource = await QueryContactsAsync(e.Text);
        }

        public async Task<MailContactContext[]> QueryContactsAsync(string text) {

            using (var database = new DatabaseContext()) {
                var candidates = await database.MailContacts
                    .Where(x => x.Address.StartsWith(text)
                                || x.Name.StartsWith(text))
                    .Take(20)
                    .ToArrayAsync();

                var contexts = candidates.Select(x => new MailContactContext(x)).ToArray();
                foreach (var context in contexts) {
                    await context.CheckSecurityStateAsync();
                }

                return contexts;
            }
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

        private async void PrepareAsReply(IReadOnlyDictionary<string, string> arguments) {
            MailContactContext from;
            MailMessageReader message;
            var id = Int64.Parse(arguments["id"]);

            using (var database = new DatabaseContext()) {
                var mime = await database.MimeMessages
                    .Where(x => x.MessageId == id)
                    .ToArrayAsync();

                if (!mime.Any())
                    throw new InvalidOperationException();

                message = new MailMessageReader(mime[0].Data);
                from = new MailContactContext(await database.MailContacts
                    .FirstAsync(x => x.Address == message.Headers.From.Address));
            }

            var context = (MailCompositionContext)DataContext;
            context.Subject = string.Format("{0} {1}", Settings.Default.PrefixForAnswering, message.Headers.Subject);
            context.Source = string.Format("asset://paranoia/message/reply?id={0}", id);

            await from.CheckSecurityStateAsync();

            RecipientsBox.Preset(new[] { from });
        }

        private async void PrepareAsReplyAll(IReadOnlyDictionary<string, string> arguments) {
            MailContactContext from;
            var carbonCopies = new List<MailContactContext>();
            var blindCarbonCopies = new List<MailContactContext>();
            MailMessageReader message;
            var id = Int64.Parse(arguments["id"]);

            using (var database = new DatabaseContext()) {
                var mime = await database.MimeMessages
                    .Where(x => x.MessageId == id)
                    .ToArrayAsync();

                if (!mime.Any())
                    throw new InvalidOperationException(Paranoia.Properties.Resources.MessageNotFoundException);

                message = new MailMessageReader(mime[0].Data);
                from = new MailContactContext(await database.MailContacts
                    .FirstAsync(x => x.Address == message.Headers.From.Address));

                foreach (var cc in message.Headers.Cc.Where(y =>
                    !App.Context.Accounts.Any(x => x.Address.EqualsIgnoreCase(y.Address)))) {
                    var lcc = cc;
                    var contact = new MailContactContext(await database.MailContacts
                        .FirstAsync(x => x.Address == lcc.Address));

                    await contact.CheckSecurityStateAsync();
                    carbonCopies.Add(contact);
                }

                foreach (var bcc in message.Headers.Bcc.Where(y =>
                    !App.Context.Accounts.Any(x => x.Address.EqualsIgnoreCase(y.Address)))) {
                    var lbcc = bcc;
                    var contact = new MailContactContext(await database.MailContacts
                        .FirstAsync(x => x.Address == lbcc.Address));

                    await contact.CheckSecurityStateAsync();
                    blindCarbonCopies.Add(contact);
                }
            }

            var context = (MailCompositionContext)DataContext;
            context.Subject = string.Format("{0} {1}", Settings.Default.PrefixForAnswering, message.Headers.Subject);
            context.Source = string.Format("asset://paranoia/message/reply?id={0}", id);

            await from.CheckSecurityStateAsync();

            RecipientsBox.Preset(new[] { from });

            CarbonCopyBox.Preset(carbonCopies);
            BlindCarbonCopyBox.Preset(blindCarbonCopies);
        }

        private async void PrepareAsForward(IReadOnlyDictionary<string, string> arguments) {
            MailMessageReader message;
            var id = Int64.Parse(arguments["id"]);

            using (var database = new DatabaseContext()) {
                var mime = await database.MimeMessages
                    .Where(x => x.MessageId == id)
                    .ToArrayAsync();

                if (!mime.Any())
                    throw new InvalidOperationException(Paranoia.Properties.Resources.MessageNotFoundException);

                message = new MailMessageReader(mime[0].Data);
            }

            var context = (MailCompositionContext)DataContext;
            context.Subject = string.Format("{0} {1}", Settings.Default.PrefixForForwarding, message.Headers.Subject);
            context.Source = string.Format("asset://paranoia/message/forward?id={0}", id);
        }

        private void DropHtmlControl(object sender, DragEventArgs e) {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var context = DataContext as MailCompositionContext;
            if (files == null | context == null)
                return;

            files.ToList().ForEach(x => context.Attachments.Add(new AttachmentContext(context, x)));
        }

        public static Window GetParentWindow(DependencyObject child) {
            while (true) {
                var parentObject = VisualTreeHelper.GetParent(child);

                if (parentObject == null) {
                    return null;
                }

                var parent = parentObject as Window;
                if (parent != null) return parent;
                child = parentObject;
            }
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
            }
        }

        #endregion

        private async Task ChangeSignatureAsync() {
            if (!HtmlControl.IsDocumentReady) {
                return;
            }

            var context = (MailCompositionContext)DataContext;
            var path = context.SelectedAccount.SignaturePath;

            if (string.IsNullOrEmpty(path) || !File.Exists(path)) {
                HtmlControl.ChangeSignature(string.Empty);
                return;
            }

            var content = await Task.Run(() => File.ReadAllText(path, Encoding.UTF8));
            HtmlControl.ChangeSignature(content);
        }

        private async void OnAccountSelectionChanged(object sender, SelectionChangedEventArgs e) {
            await ChangeSignatureAsync();
        }
    }
}