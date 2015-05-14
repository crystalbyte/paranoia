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

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public MainWindow() {
            InitializeComponent();
            DataContext = App.Context;

            CommandBindings.Add(new CommandBinding(AppCommands.Settings, OnSettings));
            CommandBindings.Add(new CommandBinding(WindowCommands.Maximize, OnMaximize));
            CommandBindings.Add(new CommandBinding(WindowCommands.Minimize, OnMinimize));
            CommandBindings.Add(new CommandBinding(WindowCommands.RestoreDown, OnRestoreDown));
            CommandBindings.Add(new CommandBinding(FlyoutCommands.Back, OnFlyoutBack));
            CommandBindings.Add(new CommandBinding(FlyoutCommands.Cancel, OnFlyoutClose));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, OnHelp));
        }

        private void OnSettings(object sender, ExecutedRoutedEventArgs e) {
            try {
                var url = typeof(AppSettingsFlyoutPage).ToPageUri();
                FlyoutFrame.Navigate(url);
                ShowFlyout();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnFlyoutClose(object sender, ExecutedRoutedEventArgs e) {
            try {
                var page = FlyoutFrame.Content as ICancelationAware;
                if (page != null) {
                    page.OnCanceled();
                }

                var context = (AppContext)DataContext;
                context.CloseFlyout();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnFlyoutBack(object sender, ExecutedRoutedEventArgs e) {
            try {
                var page = FlyoutFrame.Content as ICancelationAware;
                if (page != null) {
                    page.OnCanceled();
                }

                // BUG: The back command is currently broken in WPF 4.5 :/
                // https://connect.microsoft.com/VisualStudio/feedback/details/763996/wpf-page-navigation-looses-data-bindings
                var context = (AppContext)DataContext;
                context.CloseFlyout();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
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

        protected async override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            try {
                InvokeDeferredActions();
                InitStoryboards();
                HookUpNavigationRequests();

                await App.Context.RunAsync();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private static void InvokeDeferredActions() {
            while (DeferredActions.HasActions) {
                var action = DeferredActions.Pop();
                try {
                    action();
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            }
        }

        private void InitStoryboards() {
            _slideInOverlayStoryboard = (Storyboard)Resources["FlyoutSlideInStoryboard"];
            _slideInOverlayStoryboard.Completed += OnSlideInOverlayCompleted;

            _slideOutOverlayStoryboard = (Storyboard)Resources["FlyoutSlideOutStoryboard"];
            _slideOutOverlayStoryboard.Completed += OnSlideOutOverlayCompleted;
        }

        private void OnSlideInOverlayCompleted(object sender, EventArgs e) {
            try {
                var page = FlyoutFrame.Content as IAnimationAware;
                if (page != null) {
                    page.OnAnimationFinished();
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnSlideOutOverlayCompleted(object sender, EventArgs e) {
            try {
                FlyoutOverlay.Visibility = Visibility.Collapsed;
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void HookUpNavigationRequests() {
            var app = App.Context;
            app.FlyoutCloseRequested += OnFlyoutCloseRequested;
            app.ModalNavigationRequested += OnPopupNavigationRequested;
            app.FlyoutNavigationRequested += OnFlyoutNavigationRequested;
        }

        private void OnFlyoutCloseRequested(object sender, EventArgs e) {
            try {
                if (!IsFlyoutVisible) {
                    return;
                }
                CloseFlyout();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnPopupNavigationRequested(object sender, NavigationRequestedEventArgs e) {
            try {
                PopupFrame.Navigate(e.Target);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnFlyoutNavigationRequested(object sender, NavigationRequestedEventArgs e) {
            try {
                FlyoutFrame.Navigate(e.Target);
                ShowFlyout();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
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
            get { return (bool)GetValue(IsFlyoutVisibleProperty); }
            set { SetValue(IsFlyoutVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsOverlayVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFlyoutVisibleProperty =
            DependencyProperty.Register("IsFlyoutVisible", typeof(bool), typeof(MainWindow),
                new PropertyMetadata(false, OnIsOverlayChanged));

        #endregion

        private static void OnIsOverlayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            try {
                var window = (MainWindow)d;
                window.OnFlyoutVisibilityChanged();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnFlyoutFrameNavigated(object sender, NavigationEventArgs e) {
            try {
                var page = FlyoutFrame.Content as INavigationAware;
                if (page != null) {
                    page.OnNavigated(e);
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnFlyoutFrameNavigating(object sender, NavigatingCancelEventArgs e) {
            try {
                var page = FlyoutFrame.Content as INavigationAware;
                if (page != null) {
                    page.OnNavigating(e);
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnMainMenuSelectionChanged(object sender, SelectionChangedEventArgs e) {
            try {
                if (!IsLoaded) {
                    return;
                }

                var selection = App.Context.NavigationOptions.First(x => x.IsSelected);
                MainFrame.Navigate(selection.TargetUri);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        private void OnPopupFrameNavigated(object sender, NavigationEventArgs e) {
            try {
                var page = PopupFrame.Content as INavigationAware;
                if (page != null) {
                    page.OnNavigated(e);
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        #region Implementation of OnAccentChanged

        public void OnAccentChanged() {
            try {
                BorderBrush = Application.Current.Resources[ThemeResourceKeys.AppAccentBrushKey] as Brush;
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }

        #endregion

        private void OnCompose(object sender, ExecutedRoutedEventArgs e) {
            try {
                App.Context.Compose();
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }
    }
}