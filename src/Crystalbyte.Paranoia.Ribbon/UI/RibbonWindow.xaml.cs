#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

#endregion

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// This class represents a window with an integrated ribbon.
    /// </summary>
    [TemplatePart(Name = RibbonName, Type = typeof(Ribbon))]
    [TemplatePart(Name = StatusBarName, Type = typeof(StatusBar))]
    [TemplatePart(Name = RibbonOptionsPopupName, Type = typeof(Popup))]
    [TemplatePart(Name = RibbonOptionsListName, Type = typeof(ListView))]
    public class RibbonWindow : Window {

        #region Private Fields

        private Ribbon _ribbon;
        private StatusBar _statusBar;
        private Popup _ribbonOptionsPopup;
        private ListView _ribbonOptionsList;
        private HwndSource _hwndSource;

        #endregion

        #region Xaml Support

        public const string RibbonName = "PART_Ribbon";
        public const string StatusBarName = "PART_StatusBar";
        public const string RibbonOptionsListName = "PART_RibbonOptionsList";
        public const string RibbonOptionsPopupName = "PART_RibbonOptionsPopup";

        #endregion

        #region Construction

        static RibbonWindow() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonWindow),
                new FrameworkPropertyMetadata(typeof(RibbonWindow)));
        }

        public RibbonWindow() {
            Loaded += OnWindowLoaded;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            SourceInitialized += OnSourceInitialized;

            CommandBindings.Add(new CommandBinding(WindowCommands.Close, OnClose));
            CommandBindings.Add(new CommandBinding(WindowCommands.Maximize, OnMaximize));
            CommandBindings.Add(new CommandBinding(WindowCommands.Minimize, OnMinimize));
            CommandBindings.Add(new CommandBinding(WindowCommands.RestoreDown, OnRestoredDown));
            CommandBindings.Add(new CommandBinding(RibbonCommands.OpenAppMenu, OnOpenAppMenu));
            CommandBindings.Add(new CommandBinding(RibbonCommands.BlendInRibbon, OnBlendInRibbon));
            CommandBindings.Add(new CommandBinding(RibbonCommands.OpenRibbonOptions, OnOpenRibbonOptions));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, OnHelp));

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
            window.SyncRibbonAppearence();
            window.SyncRibbonOptionsSelection();
        }

        public Thickness FramePadding {
            get { return (Thickness)GetValue(FramePaddingProperty); }
            set { SetValue(FramePaddingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FramePadding.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FramePaddingProperty =
            DependencyProperty.Register("FramePadding", typeof(Thickness), typeof(RibbonWindow), new PropertyMetadata(new Thickness(0)));


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

        public object StatusBarItemsSource {
            get { return GetValue(StatusBarItemsSourceProperty); }
            set { SetValue(StatusBarItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StatusBarItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusBarItemsSourceProperty =
            DependencyProperty.Register("StatusBarItemsSource", typeof(object), typeof(RibbonWindow), new PropertyMetadata(null));

        public DataTemplate StatusBarItemTemplate {
            get { return (DataTemplate)GetValue(StatusBarItemTemplateProperty); }
            set { SetValue(StatusBarItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StatusBarItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusBarItemTemplateProperty =
            DependencyProperty.Register("StatusBarItemTemplate", typeof(DataTemplate), typeof(RibbonWindow), new PropertyMetadata(null));

        public ItemsPanelTemplate StatusBarItemsPanel {
            get { return (ItemsPanelTemplate)GetValue(StatusBarItemsPanelProperty); }
            set { SetValue(StatusBarItemsPanelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StatusBarItemsPanel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusBarItemsPanelProperty =
            DependencyProperty.Register("StatusBarItemsPanel", typeof(ItemsPanelTemplate), typeof(RibbonWindow), new PropertyMetadata(null));


        public Style StatusBarContainerStyle {
            get { return (Style)GetValue(StatusBarContainerStyleProperty); }
            set { SetValue(StatusBarContainerStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StatusBarContainerStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusBarContainerStyleProperty =
            DependencyProperty.Register("StatusBarContainerStyle", typeof(Style), typeof(RibbonWindow), new PropertyMetadata(null));

        #endregion

        #region Event Handlers

        private void OnRibbonSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (RibbonVisibility == RibbonVisibility.Tabs) {
                _ribbon.IsCommandStripVisible = false;
            }
        }

        private void OnRibbonOptionsPopupMouseUp(object sender, MouseButtonEventArgs e) {
            _ribbonOptionsPopup.IsOpen = false;
        }

        private void OnRibbonOptionSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var option = (RibbonOption)_ribbonOptionsList.SelectedValue;
            RibbonVisibility = option.Visibility;
        }

        private void OnOpenAppMenu(object sender, ExecutedRoutedEventArgs e) {

        }

        private void OnBlendInRibbon(object sender, ExecutedRoutedEventArgs e) {
            _ribbon.BlendIn();
            _statusBar.BlendIn();
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var point = e.GetPosition(sender as IInputElement);
            if (!_ribbon.IsFloating)
                return;

            var hit = HitTestFloatingControls(point);
            if (hit) return;

            _ribbon.BlendOut();
            _statusBar.BlendOut();
        }

        private bool HitTestFloatingControls(Point point) {
            var visuals = new List<DependencyObject>();
            VisualTreeHelper.HitTest(this, OnFilterHitTestResult, target => {
                visuals.Add(target.VisualHit);
                return HitTestResultBehavior.Continue;
            }, new PointHitTestParameters(point));

            return visuals.Contains(_ribbon) || visuals.Contains(_statusBar);
        }

        private static HitTestFilterBehavior OnFilterHitTestResult(DependencyObject target) {
            if (target is Ribbon) {
                return HitTestFilterBehavior.ContinueSkipChildren;
            }

            if (target is StatusBar) {
                return HitTestFilterBehavior.ContinueSkipChildren;
            }

            return HitTestFilterBehavior.Continue;
        }

        private void OnOpenRibbonOptions(object sender, ExecutedRoutedEventArgs e) {
            _ribbonOptionsPopup.PlacementTarget = (UIElement)e.OriginalSource;
            _ribbonOptionsPopup.IsOpen = true;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e) {
            UpdateWindowStates();
        }

        private void OnMinimize(object sender, ExecutedRoutedEventArgs e) {
            WindowState = WindowState.Minimized;
            e.Handled = true;
        }

        private void OnHelp(object sender, ExecutedRoutedEventArgs e) {
            MessageBox.Show("Help");
        }

        private void OnMaximize(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Maximized;
            e.Handled = true;
        }

        private void OnClose(object sender, RoutedEventArgs e) {
            Close();
            e.Handled = true;
        }

        private void OnRestoredDown(object sender, RoutedEventArgs e) {
            if (RibbonVisibility == RibbonVisibility.Hidden) {
                RibbonVisibility = RibbonVisibility.Tabs;
            }
            WindowState = WindowState.Normal;
            e.Handled = true;
        }

        private void OnSourceInitialized(object sender, EventArgs e) {
            var helper = new WindowInteropHelper(this);
            _hwndSource = HwndSource.FromHwnd(helper.Handle);
        }

        #endregion

        #region Class Overrides

        protected override void OnStateChanged(EventArgs e) {
            base.OnStateChanged(e);
            UpdateWindowStates();
            UpdateWindowBounds();
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            if (_ribbon != null) {
                _ribbon.SelectionChanged -= OnRibbonSelectionChanged;
            }

            _ribbon = (Ribbon)Template.FindName(RibbonName, this);
            _ribbon.SelectionChanged += OnRibbonSelectionChanged;

            _statusBar = (StatusBar)Template.FindName(StatusBarName, this);

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

            SyncRibbonOptionsSelection();
        }

        #endregion

        private void SyncRibbonAppearence() {
            switch (RibbonVisibility) {
                case RibbonVisibility.Tabs:
                    _ribbon.IsCommandStripVisible = false;
                    _ribbon.IsWindowCommandStripVisible = false;
                    _ribbon.SnapIn();
                    _statusBar.SnapIn();
                    break;
                case RibbonVisibility.TabsAndCommands:
                    _ribbon.IsCommandStripVisible = true;
                    _ribbon.IsWindowCommandStripVisible = false;
                    _ribbon.SnapIn();
                    _statusBar.SnapIn();
                    break;
                case RibbonVisibility.Hidden:
                    WindowState = WindowState.Maximized;
                    _ribbon.IsCommandStripVisible = true;
                    _ribbon.IsWindowCommandStripVisible = true;
                    _ribbon.SnapOut();
                    _statusBar.SnapOut();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SyncRibbonOptionsSelection() {
            var option = _ribbonOptionsList.Items
                .OfType<RibbonOption>()
                .First(x => x.Visibility == RibbonVisibility);

            option.IsSelected = true;
        }

        private void UpdateWindowStates() {
            IsNormalized = WindowState == WindowState.Normal;
            IsMaximized = WindowState == WindowState.Maximized;
        }

        #region Native Window Support

        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedMember.Local
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable MemberCanBePrivate.Local

        const int MONITOR_DEFAULTTONEAREST = 2;

        private void UpdateWindowBounds() {
            if (WindowState == WindowState.Normal) {
                BorderThickness = new Thickness(1);
                FramePadding = new Thickness(0);
                return;
            }

            var monitor = SafeNativeMethods.MonitorFromWindow(_hwndSource.Handle, MONITOR_DEFAULTTONEAREST);
            var info = new MONITORINFOEX { cbSize = Marshal.SizeOf(typeof(MONITORINFOEX)) };
            SafeNativeMethods.GetMonitorInfo(new HandleRef(this, monitor), ref info);

            if (_hwndSource.CompositionTarget == null) {
                throw new NullReferenceException("_hwndSource.CompositionTarget == null");
            }

            // All points queried from the Win32 API are not DPI aware.
            // Since WPF is DPI aware, one WPF pixel does not necessarily correspond to a device pixel.
            // In order to convert device pixels (Win32 API) into screen independent pixels (WPF), 
            // the following transformation must be applied to points queried using the Win32 API.
            var matrix = _hwndSource.CompositionTarget.TransformFromDevice;

            // Not DPI aware
            var workingArea = info.rcWork;
            var monitorRect = info.rcMonitor;

            // DPI aware
            var bounds = matrix.Transform(new Point(workingArea.right - workingArea.left,
                    workingArea.bottom - workingArea.top));

            // DPI aware
            var origin = matrix.Transform(new Point(workingArea.left, workingArea.top))
                - matrix.Transform(new Point(monitorRect.left, monitorRect.top));

            // Calulates the offset required to adjust the anchor position for the missing client frame border.
            // An additional -1 must be added to the top to perfectly fit the screen, reason is of yet unknown.
            var left = SystemParameters.WindowNonClientFrameThickness.Left
                + SystemParameters.ResizeFrameVerticalBorderWidth + origin.X;
            var top = SystemParameters.WindowNonClientFrameThickness.Top
                + SystemParameters.ResizeFrameHorizontalBorderHeight
                - SystemParameters.CaptionHeight + origin.Y - 1;

            FramePadding = new Thickness(left, top, 0, 0);
            MaxWidth = bounds.X + SystemParameters.ResizeFrameVerticalBorderWidth + SystemParameters.WindowNonClientFrameThickness.Right;
            MaxHeight = bounds.Y + SystemParameters.ResizeFrameHorizontalBorderHeight + SystemParameters.WindowNonClientFrameThickness.Bottom;
            BorderThickness = new Thickness(0);
        }

        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods {
            // To get a handle to the specified monitor
            [DllImport("user32.dll")]
            public static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);

            [DllImport("user32.dll")]
            public static extern bool GetMonitorInfo(HandleRef hmonitor, ref MONITORINFOEX monitorInfo);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFOEX {
            public int cbSize;
            public RECT rcMonitor; // Total area
            public RECT rcWork; // Working area
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
            public char[] szDevice;
        }

        // ReSharper restore InconsistentNaming
        // ReSharper restore UnusedMember.Local
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        // ReSharper restore MemberCanBePrivate.Local

        #endregion
    }
}