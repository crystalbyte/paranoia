using System.IO;
using Crystalbyte.Paranoia.Contexts;
using Crystalbyte.Paranoia.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Crystalbyte.Paranoia.Messaging.Mime;

namespace Crystalbyte.Paranoia {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {

        public MainWindow() {
            InitializeComponent();

            DataContext = App.AppContext;
            Loaded += OnLoaded;
        }

        private static async void OnLoaded(object sender, RoutedEventArgs e) {
            await App.AppContext.SyncAsync();
        }

        private async void OnMessagesSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var context = e.AddedItems.OfType<MessageContext>().FirstOrDefault();
            if (context != null) {
                var mime = await context.FetchMessageBodyAsync();
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(mime))) {
                    var message = Message.Load(stream);
                    App.AppContext.MessageBody = message.FindFirstHtmlVersion().GetBodyAsText();
                }
            }
        }
    }
}
