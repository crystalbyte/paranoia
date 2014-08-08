#region Using directives

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Crystalbyte.Paranoia.Contexts;
using Crystalbyte.Paranoia.UI;
using Crystalbyte.Paranoia.UI.Pages;

#endregion

namespace Crystalbyte.Paranoia {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {

        #region Private Fields

        private Storyboard _slideInOverlayStoryboard;
        private Storyboard _slideOutOverlayStoryboard;

        #endregion

        #region Construction

        public MainWindow() {
            DataContext = App.Context;
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(WindowCommands.CloseOverlay, OnCloseOverlay));

            if (DesignerProperties.GetIsInDesignMode(this)) {
                HtmlControl.Visibility = Visibility.Collapsed;
            }
        }

        private static void OnCloseOverlay(object sender, ExecutedRoutedEventArgs e) {
            App.Context.CloseOverlay();
        }

        #endregion

        #region Public Events

        public event EventHandler OverlayChanged;

        private void OnOverlayChanged() {
            var handler = OverlayChanged;
            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
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
        }

        private void HookUpControls() {
            //App.Context.HookUpSearchBox(SearchBox);
        }

        private void HookUpNavigationService() {
            App.Context.NavigationRequested += OnNavigationRequested;
        }

        private void OnNavigationRequested(object sender, NavigationRequestedEventArgs e) {
            ContentFrame.Navigate(e.Target);
            if (e.Target == typeof(BlankPage).ToPageUri()) {
                HideOverlay();
            } else {
                ShowOverlay();
            }
        }

        private void ShowOverlay() {
            IsOverlayVisible = true;
            Overlay.Visibility = Visibility.Visible;
            _slideInOverlayStoryboard.Begin();
        }

        private void HideOverlay() {
            IsOverlayVisible = false;
            while (ContentFrame.CanGoBack) {
                ContentFrame.NavigationService.RemoveBackEntry();    
            }
            
            _slideOutOverlayStoryboard.Begin();
        }

        #endregion

        public bool IsOverlayVisible {
            get { return (bool)GetValue(IsOverlayVisibleProperty); }
            set { SetValue(IsOverlayVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsOverlayVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsOverlayVisibleProperty =
            DependencyProperty.Register("IsOverlayVisible", typeof(bool), typeof(MainWindow), new PropertyMetadata(false, OnIsOverlayChanged));

        private static void OnIsOverlayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var window = (MainWindow)d;
            window.OnOverlayChanged();
        }

        private void OnMessageSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var view = (ListView)sender;
            var app = App.Composition.GetExport<AppContext>();
            app.SelectedMessages = view.SelectedItems.OfType<MailMessageContext>();

            var message = app.SelectedMessage;
            if (message == null)
                return;

            var container = (Control)MessagesListView.ItemContainerGenerator.ContainerFromItem(message);
            if (container != null) {
                container.Focus();
            }
        }

        private void OnContentFrameOnNavigated(object sender, NavigationEventArgs e) {
            var page = ContentFrame.Content as INavigationAware;
            if (page != null) {
                page.OnNavigated(e);
            }
        }
    }
}