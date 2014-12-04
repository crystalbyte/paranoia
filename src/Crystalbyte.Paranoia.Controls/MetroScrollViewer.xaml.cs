using System;
using System.Windows;
using System.Windows.Controls;

namespace Crystalbyte.Paranoia.UI {
    public class MetroScrollViewer : ScrollViewer {

        #region Construction

        static MetroScrollViewer() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MetroScrollViewer),
                new FrameworkPropertyMetadata(typeof(MetroScrollViewer)));
        }

        #endregion

        #region Dependency Properties

        public bool IsTop {
            get { return (bool)GetValue(IsTopProperty); }
            set { SetValue(IsTopProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTop.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTopProperty =
            DependencyProperty.Register("IsTop", typeof(bool), typeof(MetroScrollViewer), new PropertyMetadata(false));


        public bool IsBottom {
            get { return (bool)GetValue(IsBottomProperty); }
            set { SetValue(IsBottomProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsBottom.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsBottomProperty =
            DependencyProperty.Register("IsBottom", typeof(bool), typeof(MetroScrollViewer), new PropertyMetadata(false));

        #endregion

        #region Class Overrides

        protected override void OnScrollChanged(ScrollChangedEventArgs e) {
            base.OnScrollChanged(e);

            var scrollViewer = (ScrollViewer)e.OriginalSource;
            IsTop = scrollViewer.ScrollableHeight > 0 && Math.Abs(scrollViewer.VerticalOffset) < double.Epsilon;
            IsBottom = scrollViewer.ScrollableHeight > 0 &&
                       Math.Abs(scrollViewer.VerticalOffset + scrollViewer.ViewportHeight - scrollViewer.ExtentHeight) < double.Epsilon;
        }

        #endregion
    }
}
