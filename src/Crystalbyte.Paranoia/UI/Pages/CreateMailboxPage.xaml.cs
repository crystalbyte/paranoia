using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI.Pages {
    /// <summary>
    /// Interaction logic for CreateMailboxPage.xaml
    /// </summary>
    public partial class CreateMailboxPage {
        public CreateMailboxPage() {
            InitializeComponent();
            var parent = (IMailboxCreator) NavigationStore.Pop(typeof(CreateMailboxPage));
            DataContext = new CreateMailboxContext(parent);

            CommandBindings.Add(new CommandBinding(NavigationCommands.Close, OnCancel));
            CommandBindings.Add(new CommandBinding(NavigationCommands.Continue, OnContinue, OnCanContinue));
            NameTextBox.TextChanged += OnNameTextBoxTextChanged;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            NameTextBox.Focus();
        }

        private static void OnNameTextBoxTextChanged(object sender, TextChangedEventArgs e) {
            CommandManager.InvalidateRequerySuggested();
        }

        private void OnCanContinue(object sender, CanExecuteRoutedEventArgs e) {
            var context = DataContext as CreateMailboxContext;
            e.CanExecute = context != null && !string.IsNullOrWhiteSpace(context.Name);
        }

        private static void OnCancel(object sender, ExecutedRoutedEventArgs e) {
            App.Context.ClosePopup();
        }

        private async void OnContinue(object sender, ExecutedRoutedEventArgs e) {
            App.Context.ClosePopup();

            var context = (CreateMailboxContext)DataContext;
            await context.CommitAsync();
        }
    }
}
