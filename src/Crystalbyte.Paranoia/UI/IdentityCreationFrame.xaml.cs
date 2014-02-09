#region Using directives

using Crystalbyte.Paranoia.Contexts;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///   Interaction logic for CreateAccountScreen.xaml
    /// </summary>
    public partial class IdentityCreationFrame {
        public IdentityCreationFrame() {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            TransferDataContext();
        }

        private void TransferDataContext() {
            var page = NavigationFrame.Content as Page;
            if (page == null) {
                return;
            }

            page.DataContext = DataContext;
        }

        //public CreateAccountScreenContext ScreenContext {
        //    get { return DataContext as CreateAccountScreenContext; }
        //    set { DataContext = value; }
        //}
    }
}