using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Crystalbyte.Paranoia.UI;
using NLog;

namespace Crystalbyte.Paranoia {
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

            CommandBindings.Add(new CommandBinding(MessageCommands.Reply, OnReply));
            CommandBindings.Add(new CommandBinding(MessageCommands.Forward, OnForward));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, OnPrint));
        }

        #endregion

        #region Methods

        private void OnPrint(object sender, ExecutedRoutedEventArgs e) {
            var html = HtmlViewer.GetDocument();
            try {
                App.Context.Print(html);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private static void OnForward(object sender, ExecutedRoutedEventArgs e) {
            try {
                App.Context.Forward();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private static void OnReply(object sender, ExecutedRoutedEventArgs e) {
            try {
                App.Context.Reply();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public async Task InitWithMessageAsync(MailMessageContext message) {
            var context = new MessageInspectionContext(message);
            try {
                DataContext = context;
                HtmlViewer.Source = string.Format("asset://paranoia/message/{0}", message.Id);
                await context.InitAsync();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        public async Task InitWithFileAsync(FileSystemInfo file) {
            var context = new FileInspectionContext(file);
            try {
                DataContext = context;
                HtmlViewer.Source = string.Format("asset://paranoia/file?path={0}", Uri.EscapeDataString(file.FullName));
                await context.InitAsync();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        #endregion
    }
}
