using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using Crystalbyte.Paranoia.UI;
using System.ComponentModel;

namespace Crystalbyte.Paranoia.Sandbox {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow  {
        public MainWindow() {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) {
                return;
            }

            SuggestiveTextBox.ItemsSourceRequested += OnItemsSourceRequested;
        }

        private static void OnItemsSourceRequested(object sender, ItemsSourceRequestedEventArgs e) {
            Thread.Sleep(1000);
            e.ItemsSource = new[] {"Alexander Wieser", "Marvin Schluch", "Sebastian Thobe"};
        }
    }
}
