using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using NLog;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for InspectionWindow.xaml
    /// </summary>
    public partial class InspectionWindow {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public InspectionWindow() {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(MessagingCommands.Reply, OnReply));
            CommandBindings.Add(new CommandBinding(MessagingCommands.ReplyAll, OnReplyAll));
            CommandBindings.Add(new CommandBinding(MessagingCommands.Forward, OnForward));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, OnPrint));
        }

        #endregion

        #region Methods

        private void OnPrint(object sender, ExecutedRoutedEventArgs e) {
            var html = HtmlControl.GetDocument();
            try {
                App.Context.Print(html);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnForward(object sender, ExecutedRoutedEventArgs e) {
            try {
                var context = (InspectionContext)DataContext;
                context.Forward();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnReply(object sender, ExecutedRoutedEventArgs e) {
            try {
                var context = (InspectionContext)DataContext;
                context.Reply();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnReplyAll(object sender, ExecutedRoutedEventArgs e) {
            try {
                var context = (InspectionContext)DataContext;
                context.ReplyAll();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public async Task InitWithMessageAsync(MailMessageContext message) {
            var context = new MessageInspectionContext(message);
            try {
                DataContext = context;
                HtmlControl.Source = string.Format("asset://paranoia/message/{0}", message.Id);
                await context.InitAsync();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public async Task InitWithFileAsync(FileSystemInfo file) {
            var context = new FileInspectionContext(file);
            try {
                DataContext = context;
                HtmlControl.Source = string.Format("asset://paranoia/file?path={0}", Uri.EscapeDataString(file.FullName));
                await context.InitAsync();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        #endregion

        private void OnAttachmentMouseDoubleClicked(object sender, MouseButtonEventArgs e) {
            if (!IsLoaded) {
                return;
            }
            var view = (ListView)sender;
            var attachment = (AttachmentContext)view.SelectedValue;
            if (attachment == null) {
                return;
            }
            attachment.Open();
        }
    }
}
