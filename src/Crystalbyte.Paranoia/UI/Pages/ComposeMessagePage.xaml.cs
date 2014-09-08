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
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;

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

            var window = (MainWindow)Application.Current.MainWindow;
            window.FlyOutVisibilityChanged += OnFlyOutVisibilityChanged;
        }

        private static void OnShutdownRequested(object sender, EventArgs e) {
            App.Context.CloseFlyOut();
        }

        private void OnDocumentTextRequested(object sender, DocumentTextRequestedEventArgs e) {
            var html = HtmlControl.GetEditorDocument();

            e.Document = html;
        }

        private async void Reset() {
            var composition = (MailCompositionContext)DataContext;
            await composition.ResetAsync();
        }

        private void OnFlyOutVisibilityChanged(object sender, EventArgs e) {
            var window = (MainWindow)Application.Current.MainWindow;
            if (!window.IsFlyOutVisible) {
                RecipientsBox.Close();
            }
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
            using (var database = new DatabaseContext()) {
                var temp = Int64.Parse(arguments["id"]);
                var message = await database.MimeMessages
                    .Where(x => x.MessageId == temp)
                    .ToArrayAsync();

                replyMessage = new MailMessageReader(Encoding.UTF8.GetBytes(message[0].Data));
            }
            var context = (MailCompositionContext)DataContext;
            context.Subject = "RE: " + replyMessage.Headers.Subject;
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

        #region PasteHandler

        private void OnCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e) {
            e.CanExecute = false;
            //e.ContinueRouting = false;
            //e.Handled = true;

            var data = Clipboard.GetDataObject();
            if (data == null)
                return;

            //Debug stuff
            var formats = data.GetFormats();

            var image = data.GetData("System.Drawing.Bitmap") as Bitmap;
            if (image != null) {
                var file = Path.GetTempFileName();
                image.Save(file);
                HtmlControl.InsertHtmlAtCurrentPosition(string.Format("<img width=480 src=\"asset://tempImage/{0}\"></img>", file));
                return;
            }


            var html = (string)data.GetData(DataFormats.Html);
            if (html != null) {
                var htmlRegex = new Regex("<html.*?</html>",
                RegexOptions.Singleline);
                var temp = htmlRegex.Match(html).Value;

                var conditionRegex = new Regex(@"<!--\[if.*?<!\[endif]-->", RegexOptions.Singleline);
                const string imageTagRegexPattern = "<img.*?>(</img>){0,1}";
                const string srcPrepRegexPatter = "src=\".*?\"";
                temp = conditionRegex.Replace(temp, string.Empty);
                temp = temp.Replace("<![if !vml]>", string.Empty)
                    .Replace("<![endif]>", string.Empty);
                var imageTagMatches = Regex.Matches(temp, imageTagRegexPattern, RegexOptions.Singleline | RegexOptions.Compiled);
                foreach (Match match in imageTagMatches) {
                    var originalSrcFile = Regex.Match(match.Value, srcPrepRegexPatter).Value;
                    var srcFile = originalSrcFile.Replace("src=\"", string.Empty).Replace("\"", string.Empty).Replace("file:///", string.Empty);
                    if (new Uri(srcFile).IsFile && !File.Exists(srcFile))
                        throw new Exception("701");

                    temp = temp.Replace(originalSrcFile, string.Format("src=\"asset://tempImage/{0}\"", srcFile));
                }

                html = temp;
                HtmlControl.InsertHtmlAtCurrentPosition(html);
                return;
            }

            var planeText = (string)data.GetData(DataFormats.Text);
            if (planeText == null)
                return;

            HtmlControl.InsertPlaneAtCurrentPosition(planeText);



        }
        #endregion
    }
}