#region Using directives

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

#endregion

namespace Crystalbyte.Paranoia.UI {
    [TemplatePart(Name = RibbonCommandStripName, Type = typeof(Border))]
    public class Ribbon : TabControl {

        #region Private Fields

        private Border _commandStrip;

        #endregion

        #region Construction

        static Ribbon() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Ribbon), 
                new FrameworkPropertyMetadata(typeof(Ribbon)));
        }

        #endregion

        #region Xaml Support

        public const string RibbonCommandStripName = "PART_RibbonCommandStrip";

        #endregion

        #region Class Overrides

        protected override DependencyObject GetContainerForItemOverride() {
            return new RibbonTab();
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is RibbonTab;
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters parameters) {
            return new PointHitTestResult(this, parameters.HitPoint);
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

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            _commandStrip = (Border) Template.FindName(RibbonCommandStripName, this);
        }

        #endregion

        internal void BlendIn() {
            IsFloating = true;
            SetValue(Grid.RowProperty, 0);
            SetValue(Grid.RowSpanProperty, 2);
            SetValue(Panel.ZIndexProperty, 2);
            SetValue(VerticalAlignmentProperty, VerticalAlignment.Top);
            Visibility = Visibility.Visible;

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
            SetValue(VerticalAlignmentProperty, VerticalAlignment.Top);
            Visibility = Visibility.Visible;
        }

        internal void SlideInCommandStrip() {
            IsCommandStripVisible = true;
            var story = (Storyboard) _commandStrip.FindResource("CommandStripSlideInStoryboard");
            story.Begin();
        }

        internal void SnapOut() {
            Visibility = Visibility.Collapsed;
        }

        internal void ClearSelection() {
            SelectedIndex = -1;
        }

        internal void ExtendIntoContent() {
            SetValue(Grid.RowSpanProperty, 2);
        }

        internal void RetractFromContent() {
            SetValue(Grid.RowSpanProperty, 1);
        }

        internal void RestoreSelection() {
            if (HasItems && SelectedValue == null) {
                SelectedIndex = 0;    
            }
        }
    }
}