using System;
using System.IO;
using System.Threading.Tasks;
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
        }

        #endregion

        #region Methods

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
