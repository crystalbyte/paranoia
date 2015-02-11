using System;
using System.Threading;
using Crystalbyte.Paranoia.UI;
using System.ComponentModel;

namespace Crystalbyte.Paranoia.Sandbox {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public MainWindow() {
            // Initialization must be performed here,
            // before creating a WebControl.
            //if (!WebCore.IsInitialized) {
            //    WebCore.Initialize(new WebConfig {
            //        HomeURL = "http://www.awesomium.com".ToUri(),
            //    });
            //}

            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) {
                return;
            }
            
            SuggestionBox.ItemsSourceRequested += OnItemsSourceRequested;
        }

        private void OnItemsSourceRequested(object sender, ItemsSourceRequestedEventArgs e) {
            SuggestionBox.ItemsSource = new[] { "Alexander Wieser", "Marvin Schluch", "Sebastian Thobe" };
        }
    }
}
