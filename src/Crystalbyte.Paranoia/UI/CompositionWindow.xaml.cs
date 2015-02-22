using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using Crystalbyte.Paranoia.Themes;
using Microsoft.Win32;
using NLog;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for CompositionWindow.xaml
    /// </summary>
    public partial class CompositionWindow : IAccentAware {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public CompositionWindow() {
            InitializeComponent();

            var context = new MailCompositionContext();
            context.DocumentTextRequested += OnDocumentTextRequested;
            context.Finished += OnFinished;

            DataContext = context;
            CommandBindings.Add(new CommandBinding(EditingCommands.InsertAttachment, OnAttachment));
            CommandBindings.Add(new CommandBinding(EditingCommands.InsertLink, OnLink));
            CommandBindings.Add(new CommandBinding(FlyoutCommands.Cancel, OnCancel));
            CommandBindings.Add(new CommandBinding(WindowCommands.Maximize, OnMaximize));
            CommandBindings.Add(new CommandBinding(WindowCommands.Minimize, OnMinimize));
            CommandBindings.Add(new CommandBinding(WindowCommands.RestoreDown, OnRestoreDown));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, OnHelp));
        }

        #endregion

        #region Methods

        private void OnCancel(object sender, ExecutedRoutedEventArgs e) {
            CloseOverlay();
        }

        internal void CloseOverlay() {
            ModalOverlay.Visibility = Visibility.Collapsed;
            PopupFrame.Content = null;
        }

        private void OnFinished(object sender, EventArgs e) {
            Window.Close();
        }

        private async void OnDocumentTextRequested(object sender, DocumentTextRequestedEventArgs e) {
            e.Document = await HtmlEditor.GetHtmlAsync();
        }

        private void OnAccountComboboxDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var context = (MailCompositionContext)DataContext;
            AccountComboBox.SelectedValue = context.Accounts.OrderByDescending(x => x.IsDefaultTime).FirstOrDefault();
        }

        private async void OnRecipientsBoxItemsSourceRequested(object sender, ItemsSourceRequestedEventArgs e) {
            var control = (SuggestionBox)sender;
            var contacts = await QueryContactsAsync(e.Text);
            await Application.Current.Dispatcher.InvokeAsync(() => {
                control.ItemsSource = contacts;
            });
        }

        private void OnLink(object sender, ExecutedRoutedEventArgs e) {
            ModalOverlay.Visibility = Visibility.Visible;

            NavigationArguments.Push(HtmlEditor);
            var uri = typeof (InsertLinkModalPage).ToPageUri();
            PopupFrame.Navigate(uri);
        }

        private void OnAttachment(object sender, ExecutedRoutedEventArgs e) {
            var context = (MailCompositionContext) DataContext;
            context.InsertAttachments();
        }

        public void StartSendingAnimation() {
            var storyboard = (Storyboard) Resources["FlyOutStoryboard"];
            storyboard.Begin();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e) {
            Close();
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

        internal void PrepareAsNew() {
            var context = (MailCompositionContext)DataContext;
            context.Source = "message:///new";
            Loaded += OnLoadedAsNew;
        }

        private void OnLoadedAsNew(object sender, RoutedEventArgs e) {
            Application.Current.Dispatcher.Invoke(() => {
                RecipientsBox.Focus();
                Loaded -= OnLoadedAsNew;
            });
        }

        internal async Task PrepareAsReplyAsync(IReadOnlyDictionary<string, string> arguments) {
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
            context.Source = string.Format("message:///reply?id={0}", id);

            await Task.Run(async () => await @from.CheckSecurityStateAsync());
            RecipientsBox.Preset(new[] { from });
        }

        internal async Task PrepareAsReplyAllAsync(IReadOnlyDictionary<string, string> arguments) {
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
            context.Source = string.Format("message:///reply?id={0}", id);

            await Task.Run(() => from.CheckSecurityStateAsync());

            RecipientsBox.Preset(new[] { from });

            CarbonCopyBox.Preset(carbonCopies);
            BlindCarbonCopyBox.Preset(blindCarbonCopies);
        }

        internal async Task PrepareAsForwardAsync(IReadOnlyDictionary<string, string> arguments) {
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
            context.Source = string.Format("message:///forward?id={0}", id);
            Loaded += OnLoadedAsNew;
        }

        private void OnHtmlSurfaceDrop(object sender, DragEventArgs e) {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var context = DataContext as MailCompositionContext;
            if (files == null | context == null)
                return;

            files.ToList().ForEach(x => context.Attachments.Add(new FileAttachmentContext(x)));
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
            //if (!HtmlEditor.IsDocumentReady) {
            //    return;
            //}

            return;
            var context = (MailCompositionContext)DataContext;
            var path = context.SelectedAccount.SignaturePath;

            string signature;
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) {
                signature = string.Empty;
                var warning = string.Format(Paranoia.Properties.Resources.MissingSignatureTemplate, path);
                Logger.Warn(warning);
            } else {
                signature = await Task.Run(() => File.ReadAllText(path, Encoding.UTF8));
            }

            //var composition = HtmlEditor.Composition;
            //var document = new HtmlDocument();
            //document.LoadHtml(composition);

            //var node = document.DocumentNode.SelectSingleNode("//div[@id='signature']");
            //node.RemoveAllChildren();
            //node.InnerHtml = signature;

            //HtmlEditor.Composition = document.DocumentNode.WriteTo();
        }

        private async void OnAccountSelectionChanged(object sender, SelectionChangedEventArgs e) {
            try {
                await ChangeSignatureAsync();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        #endregion

        #region Implementation of OnAccentChanged

        public void OnAccentChanged() {
            BorderBrush = Application.Current.Resources[ThemeResourceKeys.AppAccentBrushKey] as Brush;
        }

        #endregion
    }
}
