#region Using directives

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;
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
    public partial class ComposeMessagePage : INavigationAware, IAnimationAware {

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
            var temp = Int64.Parse(arguments["id"]);

            using (var database = new DatabaseContext()) {
                var message = await database.MimeMessages
                    .Where(x => x.MessageId == temp)
                    .ToArrayAsync();

                if (!message.Any())
                    throw new Exception("701");

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

        #region Implementation of IAnimationAware

        public void OnAnimationFinished() {
            HtmlControl.Visibility = Visibility.Visible;
        }

        #endregion

        private void MetroCircleButton_Click(object sender, RoutedEventArgs e) {
            var window = new MetroPageHostWindow();
            window.SetContent(this);
            window.Show();
        }
    }
}