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
using NLog;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for ComposeMessagePage.xaml
    /// </summary>
    public partial class CompositionPage : INavigationAware {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public CompositionPage() {
            InitializeComponent();

            var context = new MailCompositionContext();
            context.DocumentTextRequested += OnDocumentTextRequested;
            context.Finished += OnFinished;

            HtmlControl.ScriptingFailure += OnEditorScriptingFailure;
            HtmlControl.EditorContentLoaded += OnEditorContentLoaded;
            DataContext = context;
        }

        #endregion

        #region Dependency Properties

        public bool IsEditorContentLoaded {
            get { return (bool)GetValue(IsEditorContentLoadedProperty); }
            set { SetValue(IsEditorContentLoadedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsEditorContentLoaded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEditorContentLoadedProperty =
            DependencyProperty.Register("IsEditorContentLoaded", typeof(bool), typeof(CompositionPage), new PropertyMetadata(false));

        #endregion

        private async void OnEditorContentLoaded(object sender, EditorContentLoadedEventArgs e) {
            IsEditorContentLoaded = true;

            await Task.Run(async () => {
                await Application.Current.Dispatcher.InvokeAsync(async () => {
                    await ChangeSignatureAsync();
                });
            });
        }

        private void OnHtmlControlInitialized(object sender, EventArgs e) {
            var control = (HtmlControl)sender;
            control.WebSession.ClearCache();
        }

        private static void OnEditorScriptingFailure(object sender, ScriptingFailureEventArgs e) {
            Logger.Error(e.Exception);
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
            var control = (SuggestiveTextBox)sender;
            var contacts = await QueryContactsAsync(e.Text);
            await Application.Current.Dispatcher.InvokeAsync(() => {
                control.ItemsSource = contacts;
            });
        }

        private void FocusOnPageLoad(Func<Control> controlDelegate) {
            if (IsLoaded) {
                FocusByDelegate(controlDelegate);
            } else {
                Loaded += (sender, e) => FocusByDelegate(controlDelegate);
            }
        }

        private static void FocusByDelegate(Func<Control> controlDelegate) {
            var control = controlDelegate();
            control.Focus();
        }

        public Task<MailContactContext[]> QueryContactsAsync(string text) {
            return Task.Run(async () => {
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
            });
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

            FocusOnPageLoad(() => RecipientsBox);
        }

        private async Task PrepareAsReplyAsync(IReadOnlyDictionary<string, string> arguments) {
            MailContactContext from = null;
            MailMessageReader message = null;
            var id = Int64.Parse(arguments["id"]);

            await Task.Run(async () => {
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
            });

            var context = (MailCompositionContext)DataContext;
            context.Subject = string.Format("{0} {1}", Settings.Default.PrefixForAnswering, message.Headers.Subject);
            context.Source = string.Format("asset://paranoia/message/reply?id={0}", id);

            await Task.Run(async () => await @from.CheckSecurityStateAsync());
            RecipientsBox.Preset(new[] { from });
        }

        private async Task PrepareAsReplyAllAsync(IReadOnlyDictionary<string, string> arguments) {
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

            await Task.Run(() => from.CheckSecurityStateAsync());

            RecipientsBox.Preset(new[] { from });

            CarbonCopyBox.Preset(carbonCopies);
            BlindCarbonCopyBox.Preset(blindCarbonCopies);
        }

        private async Task PrepareAsForwardAsync(IReadOnlyDictionary<string, string> arguments) {

            var id = Int64.Parse(arguments["id"]);

            var reader = await Task.Run(async () => {
                using (var database = new DatabaseContext()) {
                    var mime = await database.MimeMessages
                        .Where(x => x.MessageId == id)
                        .ToArrayAsync();

                    if (!mime.Any())
                        throw new InvalidOperationException(Paranoia.Properties.Resources.MessageNotFoundException);

                    return new MailMessageReader(mime[0].Data);
                }
            });

            var context = (MailCompositionContext)DataContext;
            context.Subject = string.Format("{0} {1}", Settings.Default.PrefixForForwarding, reader.Headers.Subject);
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
            if (IsEditorContentLoaded) {
                await ChangeSignatureAsync();
            }
        }

        #region Implementation of INavigationAware

        public async void OnNavigated(NavigationEventArgs e) {
            Reset();

            var arguments = e.Uri.OriginalString.ToPageArguments();
            if (arguments.ContainsKey("action") && arguments["action"] == "new") {
                PrepareAsNew();
                return;
            }

            if (arguments.ContainsKey("action") && arguments["action"] == "reply") {
                await PrepareAsReplyAsync(arguments);
                return;
            }

            if (arguments.ContainsKey("action") && arguments["action"] == "reply-all") {
                await PrepareAsReplyAllAsync(arguments);
                return;
            }

            if (arguments.ContainsKey("action") && arguments["action"] == "forward") {
                await PrepareAsForwardAsync(arguments);
            }
        }

        public void OnNavigating(NavigatingCancelEventArgs e) {
            // Nothing ...
        }

        #endregion
    }
}