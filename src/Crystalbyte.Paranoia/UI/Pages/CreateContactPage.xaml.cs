#region Using directives

using System.Windows;
using System.Windows.Controls;
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

            CommandBindings.Add(new CommandBinding(NavigationCommands.Close, OnCancel));
            NameTextBox.TextChanged += OnNameTextBoxTextChanged;
            Loaded += OnLoaded;
        }

        private static void OnNameTextBoxTextChanged(object sender, TextChangedEventArgs e) {
            CommandManager.InvalidateRequerySuggested();
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            NameTextBox.Focus();
        }

        private static void OnCancel(object sender, ExecutedRoutedEventArgs e) {
            App.Context.ClosePopup();
        }
    }
}