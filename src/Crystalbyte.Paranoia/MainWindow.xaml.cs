#region Using directives

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Crystalbyte.Paranoia.UI;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {

        private Storyboard _slideInOverlayStoryboard;
        private Storyboard _slideOutOverlayStoryboard;

        #region Construction

        public MainWindow() {
            DataContext = App.Context;
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(WindowCommands.CloseOverlay, OnCloseOverlay));
        }

        private void OnCloseOverlay(object sender, ExecutedRoutedEventArgs e) {
            _slideOutOverlayStoryboard.Begin();
        }

        #endregion

        #region Class Overrides

        protected override async void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            LoadResources();
            HookUpControls();
            HookUpNavigationService();

            await App.Context.RunAsync();
        }

        private void LoadResources() {
            _slideInOverlayStoryboard = (Storyboard)Resources["OverlaySlideInStoryboard"];
            _slideOutOverlayStoryboard = (Storyboard)Resources["OverlaySlideOutStoryboard"];
            _slideOutOverlayStoryboard.Completed += OnSlideOutOverlayCompleted;
        }

        private void OnSlideOutOverlayCompleted(object sender, EventArgs e) {
            Overlay.Visibility = Visibility.Collapsed;
            if (!string.IsNullOrEmpty(App.Context.Html)) {
                HtmlFrame.Visibility = Visibility.Visible;
            }
        }

        private void HookUpControls() {
            App.Context.HookUpSearchBox(SearchBox);
        }

        private void HookUpNavigationService() {
            App.Context.NavigationRequested += OnNavigationRequested;
        }

        private void OnNavigationRequested(object sender, Contexts.NavigationRequestedEventArgs e) {
            Overlay.Visibility = Visibility.Visible;
            HtmlFrame.Visibility = Visibility.Collapsed;
            ContentFrame.Navigate(e.Target);

            _slideInOverlayStoryboard.Begin();
        }

        #endregion

        private void OnMessageSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var view = (ListView) sender;
            var app = App.Composition.GetExport<AppContext>();
            app.SelectedMessages = view.SelectedItems.OfType<MailMessageContext>();

            var message = app.SelectedMessage;
            if (message == null) 
                return;

            var container = (Control) MessagesListView.ItemContainerGenerator.ContainerFromItem(message);
            if (container != null) {
                container.Focus();
            }
        }
    }
}