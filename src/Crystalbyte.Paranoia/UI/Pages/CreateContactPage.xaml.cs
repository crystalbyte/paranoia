#region Using directives

using System.Windows;
using System.Windows.Input;

#endregion

namespace Crystalbyte.Paranoia.UI.Pages {
    /// <summary>
    ///     Interaction logic for CreateContactPage.xaml
    /// </summary>
    public partial class CreateContactPage {
        public CreateContactPage() {
            InitializeComponent();
            DataContext = new CreateContactContext();

            CommandBindings.Add(new CommandBinding(PageCommands.Cancel, OnCancel));
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            NameTextBox.Focus();
        }

        private void OnCancel(object sender, ExecutedRoutedEventArgs e) {
            App.Context.ClosePopup();
        }
    }
}