#region Using directives

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

#endregion

namespace Crystalbyte.Paranoia.UI {
    public class Ribbon : TabControl {

        #region Construction

        static Ribbon() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Ribbon), 
                new FrameworkPropertyMetadata(typeof(Ribbon)));
        }

        #endregion

        #region Class Overrides

        protected override DependencyObject GetContainerForItemOverride() {
            return new RibbonTab();
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is RibbonTab;
        }

        #endregion

        #region Dependency Properties

        public bool IsFloating {
            get { return (bool)GetValue(IsFloatingProperty); }
            set { SetValue(IsFloatingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFloating.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFloatingProperty =
            DependencyProperty.Register("IsFloating", typeof(bool), typeof(Ribbon), new PropertyMetadata(false));

        public bool IsCommandStripVisible {
            get { return (bool)GetValue(IsCommandStripVisibleProperty); }
            set { SetValue(IsCommandStripVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HideCommandStrip.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCommandStripVisibleProperty =
            DependencyProperty.Register("IsCommandStripVisible", typeof(bool), typeof(Ribbon), new PropertyMetadata(true));

        public bool IsWindowCommandStripVisible {
            get { return (bool)GetValue(IsWindowCommandStripVisibleProperty); }
            set { SetValue(IsWindowCommandStripVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsWindowCommandBarVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsWindowCommandStripVisibleProperty =
            DependencyProperty.Register("IsWindowCommandStripVisible", typeof(bool), typeof(Ribbon), new PropertyMetadata(false));


        public string AppMenuText {
            get { return (string)GetValue(AppMenuTextProperty); }
            set { SetValue(AppMenuTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AppMenuButtonContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AppMenuTextProperty =
            DependencyProperty.Register("AppMenuText", typeof(string), typeof(Ribbon),
                new PropertyMetadata(string.Empty));

        #endregion

        #region Class Overrides

        protected override HitTestResult HitTestCore(PointHitTestParameters parameters) {
            return new PointHitTestResult(this, parameters.HitPoint);
        }

        #endregion

        internal void BlendIn() {
            IsFloating = true;
            Visibility = Visibility.Visible;
            SetValue(Grid.RowProperty, 0);
            SetValue(Grid.RowSpanProperty, 2);
            SetValue(Panel.ZIndexProperty, 2);
            SetValue(VerticalAlignmentProperty, VerticalAlignment.Top);

            // The sequence of calls in this method is important.
            // Don't change the layout after the animation has started.
            // The storyboard must be invoked last. 
            var story = (Storyboard) FindResource("RibbonBlendInStoryboard");
            story.Begin();
        }

        internal void BlendOut() {
            if (!IsFloating) {
                return;
            }
            IsFloating = false;
            Visibility = Visibility.Collapsed;
        }

        internal void SnapIn() {
            IsFloating = false;
            SetValue(Grid.RowProperty, 1);
            SetValue(Grid.RowSpanProperty, 1);
            SetValue(Panel.ZIndexProperty, 0);
            SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            Visibility = Visibility.Visible;
        }

        internal void SnapOut() {
            Visibility = Visibility.Collapsed;
        }
    }
}