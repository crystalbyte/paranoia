using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for CreateMailboxModalPage.xaml
    /// </summary>
    public partial class CreateMailboxModalPage {
        public CreateMailboxModalPage() {
            InitializeComponent();
            var parent = (IMailboxCreator) NavigationArguments.Pop();
            DataContext = new CreateMailboxContext(parent);

            CommandBindings.Add(new CommandBinding(FlyoutCommands.Cancel, OnCancel));
            CommandBindings.Add(new CommandBinding(FlyoutCommands.Accept, OnAccept, OnCanAccept));
            NameTextBox.TextChanged += OnNameTextBoxTextChanged;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            NameTextBox.Focus();
        }

        private static void OnNameTextBoxTextChanged(object sender, TextChangedEventArgs e) {
            CommandManager.InvalidateRequerySuggested();
        }

        private void OnCanAccept(object sender, CanExecuteRoutedEventArgs e) {
            var context = (CreateMailboxContext) DataContext;
            e.CanExecute = context.IsValid;
        }

        private static void OnCancel(object sender, ExecutedRoutedEventArgs e) {
            App.Context.ClosePopup();
        }

        private async void OnAccept(object sender, ExecutedRoutedEventArgs e) {
            App.Context.ClosePopup();

            var context = (CreateMailboxContext)DataContext;
            await context.CommitAsync();
        }
    }
}
