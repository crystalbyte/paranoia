#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using Directives

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Crystalbyte.Paranoia.Themes;
using NLog;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IAccentAware {
        #region Private Fields

        private Storyboard _slideInOverlayStoryboard;
        private Storyboard _slideOutOverlayStoryboard;
        private Storyboard _slideOutMainFrameStoryboard;
        private Storyboard _slideInMainFrameStoryboard;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public MainWindow() {
            InitializeComponent();
            DataContext = App.Context;
            Loaded += OnLoaded;

            CommandBindings.Add(new CommandBinding(AppCommands.Settings, OnSettings));
            CommandBindings.Add(new CommandBinding(WindowCommands.Maximize, OnMaximize));
            CommandBindings.Add(new CommandBinding(WindowCommands.Minimize, OnMinimize));
            CommandBindings.Add(new CommandBinding(WindowCommands.RestoreDown, OnRestoreDown));
            CommandBindings.Add(new CommandBinding(FlyoutCommands.Back, OnFlyoutBack));
            CommandBindings.Add(new CommandBinding(FlyoutCommands.Cancel, OnFlyoutClose));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, OnHelp));
        }

        private static async void OnLoaded(object sender, RoutedEventArgs e) {
            await App.Context.RunAsync();
        }

        private void OnSettings(object sender, ExecutedRoutedEventArgs e) {
            var url = typeof (AppSettingsFlyoutPage).ToPageUri();
            FlyoutFrame.Navigate(url);
            ShowFlyout();
        }

        private void OnFlyoutClose(object sender, ExecutedRoutedEventArgs e) {
            var page = FlyoutFrame.Content as ICancelationAware;
            if (page != null) {
                page.OnCanceled();
            }

            var context = (AppContext) DataContext;
            context.CloseFlyout();
        }

        private void OnFlyoutBack(object sender, ExecutedRoutedEventArgs e) {
            var page = FlyoutFrame.Content as ICancelationAware;
            if (page != null) {
                page.OnCanceled();
            }

            // BUG: The back command is currently broken in WPF 4.5 :/
            // https://connect.microsoft.com/VisualStudio/feedback/details/763996/wpf-page-navigation-looses-data-bindings
            var context = (AppContext) DataContext;
            context.CloseFlyout();
        }

        #endregion

        #region Public Events

        public event EventHandler FlyoutVisibilityChanged;

        private void OnFlyoutVisibilityChanged() {
            var handler = FlyoutVisibilityChanged;
            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Class Overrides

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            InvokeDeferredActions();
            InitStoryboards();
            HookUpNavigationRequests();

            if (DesignerProperties.GetIsInDesignMode(this)) {
                MainFrame.Source =
                    new Uri(
                        "http://www.fantasystronghold.de/news/wp-content/uploads/2014/02/MyLittlePony_splash_2048x1536_EN.jpg",
                        UriKind.Absolute);
            }
        }

        private static void InvokeDeferredActions() {
            while (DeferredActions.HasActions) {
                var action = DeferredActions.Pop();
                try {
                    action();
                }
                catch (Exception ex) {
                    Logger.Error(ex);
                }
            }
        }

        private void InitStoryboards() {
            _slideInOverlayStoryboard = (Storyboard) Resources["FlyoutSlideInStoryboard"];
            _slideInOverlayStoryboard.Completed += OnSlideInOverlayCompleted;

            _slideOutOverlayStoryboard = (Storyboard) Resources["FlyoutSlideOutStoryboard"];
            _slideOutOverlayStoryboard.Completed += OnSlideOutOverlayCompleted;

            _slideOutMainFrameStoryboard = (Storyboard) Resources["MainFrameSlideOutStoryboard"];
            _slideOutMainFrameStoryboard.Completed += OnSlideOutMainFrameCompleted;

            _slideInMainFrameStoryboard = (Storyboard) Resources["MainFrameSlideInStoryboard"];
            _slideInMainFrameStoryboard.Completed += OnSlideInMainFrameCompleted;
        }

        private static void OnSlideInMainFrameCompleted(object sender, EventArgs e) {
            App.Context.IsAnimating = false;
        }

        private void OnSlideOutMainFrameCompleted(object sender, EventArgs e) {
            var selection = App.Context.NavigationOptions.First(x => x.IsSelected);
            MainFrame.Navigate(selection.TargetUri);
            _slideInMainFrameStoryboard.Begin();
        }

        private void OnSlideInOverlayCompleted(object sender, EventArgs e) {
            var page = FlyoutFrame.Content as IAnimationAware;
            if (page != null) {
                page.OnAnimationFinished();
            }
        }

        private void OnSlideOutOverlayCompleted(object sender, EventArgs e) {
            FlyoutOverlay.Visibility = Visibility.Collapsed;
        }

        private void HookUpNavigationRequests() {
            var app = App.Context;
            app.FlyoutCloseRequested += OnFlyoutCloseRequested;
            app.ModalNavigationRequested += OnPopupNavigationRequested;
            app.FlyoutNavigationRequested += OnFlyoutNavigationRequested;
        }

        private void OnFlyoutCloseRequested(object sender, EventArgs e) {
            if (!IsFlyoutVisible) {
                return;
            }
            CloseFlyout();
        }

        private void OnPopupNavigationRequested(object sender, NavigationRequestedEventArgs e) {
            PopupFrame.Navigate(e.Target);
        }

        private void OnFlyoutNavigationRequested(object sender, NavigationRequestedEventArgs e) {
            FlyoutFrame.Navigate(e.Target);
            ShowFlyout();
        }

        private void ShowFlyout() {
            IsFlyoutVisible = true;
            FlyoutOverlay.Visibility = Visibility.Visible;
            _slideInOverlayStoryboard.Begin();
        }

        private void CloseFlyout() {
            IsFlyoutVisible = false;

            FlyoutFrame.Content = null;
            while (FlyoutFrame.CanGoBack) {
                FlyoutFrame.NavigationService.RemoveBackEntry();
            }
            _slideOutOverlayStoryboard.Begin();
        }

        #endregion

        #region Dependency Properties

        public bool IsFlyoutVisible {
            get { return (bool) GetValue(IsFlyoutVisibleProperty); }
            set { SetValue(IsFlyoutVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsOverlayVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFlyoutVisibleProperty =
            DependencyProperty.Register("IsFlyoutVisible", typeof (bool), typeof (MainWindow),
                new PropertyMetadata(false, OnIsOverlayChanged));

        #endregion

        private static void OnIsOverlayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var window = (MainWindow) d;
            window.OnFlyoutVisibilityChanged();
        }

        private void OnFlyoutFrameNavigated(object sender, NavigationEventArgs e) {
            var page = FlyoutFrame.Content as INavigationAware;
            if (page != null) {
                page.OnNavigated(e);
            }
        }

        private void OnFlyoutFrameNavigating(object sender, NavigatingCancelEventArgs e) {
            var page = FlyoutFrame.Content as INavigationAware;
            if (page != null) {
                page.OnNavigating(e);
            }
        }

        private void OnMainMenuSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!IsLoaded) {
                return;
            }

            var view = (ListView) sender;
            var selection = view.SelectedValue as NavigationContext;
            if (selection == null) {
                return;
            }

            App.Context.IsAnimating = true;
            _slideOutMainFrameStoryboard.Begin();
        }

        private void OnPopupFrameNavigated(object sender, NavigationEventArgs e) {
            var page = PopupFrame.Content as INavigationAware;
            if (page != null) {
                page.OnNavigated(e);
            }
        }

        #region Implementation of OnAccentChanged

        public void OnAccentChanged() {
            BorderBrush = Application.Current.Resources[ThemeResourceKeys.AppAccentBrushKey] as Brush;
        }

        #endregion

        private void OnCompose(object sender, ExecutedRoutedEventArgs e) {
            try {
                App.Context.Compose();
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }
        }
    }
}