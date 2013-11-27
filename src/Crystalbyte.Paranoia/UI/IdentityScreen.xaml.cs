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
using Crystalbyte.Paranoia.Contexts;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for IdentityScreen.xaml
    /// </summary>
    public partial class IdentityScreen {
        public IdentityScreen() {
            InitializeComponent();
            IsVisibleChanged += OnIsVisibleChanged;
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (!((bool) e.NewValue)) 
                return;

            NameTextBox.Focusable = true;
            Keyboard.Focus(NameTextBox);
        }
    }
}
