using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for CreateMailboxModalPage.xaml
    /// </summary>
    public partial class InsertLinkModalPage {

        public InsertLinkModalPage() {
            InitializeComponent();
            var parent = (HtmlEditor) NavigationArguments.Pop();
            DataContext = new InsertLinkContext(parent);

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
            var context = (InsertLinkContext)DataContext;
            e.CanExecute = context.IsValid;
        }

        private void Cancel() {
            var window = Window.GetWindow(this) as CompositionWindow;
            if (window != null) {
                window.CloseOverlay();
            }
        }

        private void OnCancel(object sender, ExecutedRoutedEventArgs e) {
            Cancel();
        }

        private async void OnAccept(object sender, ExecutedRoutedEventArgs e) {
            Cancel();

            var context = (InsertLinkContext)DataContext;
            await context.CommitAsync();
        }
    }
}
