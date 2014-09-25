using Crystalbyte.Paranoia.UI;
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
using System.Windows.Shapes;

namespace Crystalbyte.Paranoia {
    /// <summary>
    /// Interaktionslogik für MetroPageHostWindow.xaml
    /// </summary>
    public partial class MetroPageHostWindow : MetroWindow {
        public MetroPageHostWindow() {
            InitializeComponent();

            var mainwindow = Application.Current.MainWindow;
            Owner = mainwindow;
            Height = mainwindow.Height * 0.8;
            Width = mainwindow.Width * 0.8;
        }

        public void SetContent(object content) {
            PageHost.Content = content;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            this.WindowState = WindowState.Minimized;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            this.WindowState = WindowState.Maximized;
        }
    }
}
