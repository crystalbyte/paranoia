#region Using directives

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;

#endregion

namespace Crystalbyte.Paranoia.UI {
    [TemplatePart(Name = RibbonName, Type = typeof(Ribbon))]
    [TemplatePart(Name = RibbonOptionsPopupName, Type = typeof(Popup))]
    [TemplatePart(Name = RibbonOptionsListName, Type = typeof(ListView))]
    [TemplatePart(Name = RibbonPopupName, Type = typeof(Popup))]
    public class RibbonWindow : Window {

        #region Private Fields

        private Ribbon _ribbon;
        private Popup _ribbonPopup;
        private Popup _ribbonOptionsPopup;
        private ListView _ribbonOptionsList;

        #endregion

        #region Xaml Support

        public const string RibbonName = "PART_Ribbon";
        public const string RibbonOptionsListName = "PART_RibbonOptionsList";
        public const string RibbonOptionsPopupName = "PART_RibbonOptionsPopup";
        public const string RibbonPopupName = "PART_RibbonPopup";

        #endregion

        #region Construction

        static RibbonWindow() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonWindow),
                new FrameworkPropertyMetadata(typeof(RibbonWindow)));
        }

        public RibbonWindow() {
            Loaded += OnWindowLoaded;
            // We need to set the height for the window to stay ontop the Taskbar
            MaxHeight = SystemParameters.WorkArea.Height
                        + SystemParameters.WindowResizeBorderThickness.Top
                        + SystemParameters.WindowResizeBorderThickness.Bottom;

            CommandBindings.Add(new CommandBinding(WindowCommands.Close, OnClosed));
            CommandBindings.Add(new CommandBinding(WindowCommands.Maximize, OnMaximized));
            CommandBindings.Add(new CommandBinding(WindowCommands.Minimize, OnMinimized));
            CommandBindings.Add(new CommandBinding(WindowCommands.RestoreDown, OnRestoredDown));
            CommandBindings.Add(new CommandBinding(RibbonCommands.OpenAppMenu, OnAppMenuInvoked));
            CommandBindings.Add(new CommandBinding(WindowCommands.BlendInRibbon, OnBlendInRibbon));

            Tabs = new RibbonTabCollection();
        }

        #endregion

        #region Public Events

        public event EventHandler RibbonVisibilityChanged;

        protected virtual void OnRibbonVisibilityChanged() {
            var handler = RibbonVisibilityChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion

        #region Dependency Properties

        public RibbonVisibility RibbonVisibility {
            get { return (RibbonVisibility)GetValue(RibbonVisibilityProperty); }
            set { SetValue(RibbonVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RibbonVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RibbonVisibilityProperty =
            DependencyProperty.Register("RibbonVisibility", typeof(RibbonVisibility), typeof(RibbonWindow),
                new PropertyMetadata(RibbonVisibility.Tabs, OnRibbonVisibilityChanged));

        private static void OnRibbonVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var window = (RibbonWindow)d;
            window.OnRibbonVisibilityChanged();
            window.SyncVisibilitySelection();
        }

        public bool IsNormalized {
            get { return (bool)GetValue(IsNormalizedProperty); }
            set { SetValue(IsNormalizedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for QuickAccessCommands.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsNormalizedProperty =
            DependencyProperty.Register("IsNormalized", typeof(bool), typeof(RibbonWindow),
                new PropertyMetadata(false));

        public bool IsMaximized {
            get { return (bool)GetValue(IsMaximizedProperty); }
            set { SetValue(IsMaximizedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMaximized.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMaximizedProperty =
            DependencyProperty.Register("IsMaximized", typeof(bool), typeof(RibbonWindow), new PropertyMetadata(false));

        public RibbonTabCollection Tabs {
            get { return (RibbonTabCollection)GetValue(TabsProperty); }
            set { SetValue(TabsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Tabs.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TabsProperty =
            DependencyProperty.Register("Tabs", typeof(RibbonTabCollection), typeof(RibbonWindow), new PropertyMetadata(null));

        public Style TabStyle {
            get { return (Style)GetValue(TabStyleProperty); }
            set { SetValue(TabStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RibbonTabStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TabStyleProperty =
            DependencyProperty.Register("TabStyle", typeof(Style), typeof(RibbonWindow), new PropertyMetadata(null));

        #endregion

        #region Event Handlers

        private void OnRibbonSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (RibbonVisibility == RibbonVisibility.Tabs) {
                OpenCommandsPopup();
            }
        }

        private void OnRibbonOptionsPopupMouseUp(object sender, MouseButtonEventArgs e) {
            _ribbonOptionsPopup.IsOpen = false;
        }

        private void OnRibbonOptionSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var option = (RibbonOption)_ribbonOptionsList.SelectedValue;

            if (RibbonVisibility == option.Visibility) {
                return;
            }

            RibbonVisibility = option.Visibility;
            if (RibbonVisibility == RibbonVisibility.Hidden) {
                WindowState = WindowState.Maximized;
            }
        }

        private void OnAppMenuInvoked(object sender, ExecutedRoutedEventArgs e) {

        }

        private void OnBlendInRibbon(object sender, ExecutedRoutedEventArgs e) {
            _ribbonPopup.IsOpen = true;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e) {
            UpdateWindowStates();
        }

        private void OnMinimized(object sender, ExecutedRoutedEventArgs e) {
            WindowState = WindowState.Minimized;
            e.Handled = true;
        }

        private void OnMaximized(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Maximized;
            e.Handled = true;
        }

        private void OnClosed(object sender, RoutedEventArgs e) {
            Close();
            e.Handled = true;
        }

        private void OnRestoredDown(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Normal;
            e.Handled = true;
        }

        #endregion

        #region Class Overrides

        protected override void OnStateChanged(EventArgs e) {
            base.OnStateChanged(e);
            UpdateWindowPadding();
            UpdateWindowStates();
        }

        private void OnRibbonCommandsPopupClosed(object sender, EventArgs e) {

        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            if (_ribbon != null) {
                _ribbon.SelectionChanged -= OnRibbonSelectionChanged;
            }

            _ribbon = (Ribbon)Template.FindName(RibbonName, this);
            _ribbon.SelectionChanged += OnRibbonSelectionChanged;

            if (_ribbonPopup != null) {
                _ribbonPopup.Closed -= OnRibbonCommandsPopupClosed;
            }
            _ribbonPopup = (Popup)Template.FindName(RibbonPopupName, this);
            _ribbonPopup.Closed += OnRibbonCommandsPopupClosed;

            if (_ribbonOptionsPopup != null) {
                _ribbonOptionsPopup.MouseUp -= OnRibbonOptionsPopupMouseUp;
            }

            _ribbonOptionsPopup = (Popup)Template.FindName(RibbonOptionsPopupName, this);
            _ribbonOptionsPopup.MouseUp += OnRibbonOptionsPopupMouseUp;

            if (_ribbonOptionsList != null) {
                _ribbonOptionsList.SelectionChanged -= OnRibbonOptionSelectionChanged;
            }

            _ribbonOptionsList = (ListView)Template.FindName(RibbonOptionsListName, this);
            _ribbonOptionsList.SelectionChanged += OnRibbonOptionSelectionChanged;
            _ribbonOptionsList.Items.AddRange(new[] {
                new RibbonOption {
                    Title = Properties.Resources.AutoHideRibbonTitle, 
                    Description = Properties.Resources.AutoHideRibbonDescription,
                    Visibility = RibbonVisibility.Hidden,
                    ImageSource = new BitmapImage(new Uri("/Crystalbyte.Paranoia.Ribbon;component/Assets/autohide.png", UriKind.Relative))
                },
                new RibbonOption {
                    Title = Properties.Resources.ShowTabsTitle, 
                    Description = Properties.Resources.ShowTabsDescription,
                    Visibility = RibbonVisibility.Tabs,
                    ImageSource = new BitmapImage(new Uri("/Crystalbyte.Paranoia.Ribbon;component/Assets/show.tabs.png", UriKind.Relative))
                },
                new RibbonOption {
                    Title = Properties.Resources.ShowTabsAndCommandsTitle, 
                    Description = Properties.Resources.ShowTabsAndCommandsDescription,
                    Visibility = RibbonVisibility.TabsAndCommands,
                    ImageSource = new BitmapImage(new Uri("/Crystalbyte.Paranoia.Ribbon;component/Assets/show.tabs.commands.png", UriKind.Relative))
                }
            });
        }

        #endregion

        private void OpenCommandsPopup() {
            // Detach ribbon from window.
            //_ribbonWindowContentHost.Children.Remove(_ribbon);

            // Attach ribbon to popup.
            _ribbonPopup.Child = _ribbon;
            _ribbonPopup.IsOpen = true;
        }

        private void SyncVisibilitySelection() {
            var option = _ribbonOptionsList.Items
                .OfType<RibbonOption>()
                .First(x => x.Visibility == RibbonVisibility);

            option.IsSelected = true;
        }

        private void UpdateWindowStates() {
            IsNormalized = WindowState == WindowState.Normal;
            IsMaximized = WindowState == WindowState.Maximized;
        }

        private void UpdateWindowPadding() {
            if (WindowState == WindowState.Normal) {
                Padding = new Thickness(0);
            } else {
                Padding = new Thickness(
                    SystemParameters.WindowResizeBorderThickness.Left +
                    SystemParameters.WindowNonClientFrameThickness.Left,
                    SystemParameters.WindowResizeBorderThickness.Top +
                    SystemParameters.WindowNonClientFrameThickness.Top - SystemParameters.CaptionHeight,
                    SystemParameters.WindowResizeBorderThickness.Right +
                    SystemParameters.WindowNonClientFrameThickness.Right, 0);
            }
        }
    }
}