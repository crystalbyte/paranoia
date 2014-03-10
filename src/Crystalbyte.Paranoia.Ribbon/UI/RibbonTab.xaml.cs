#region Using directives

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

#endregion

namespace Crystalbyte.Paranoia.UI {
    [ContentProperty("Header")]
    public class RibbonTab : Control {
        #region Construction

        static RibbonTab() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (RibbonTab),
                new FrameworkPropertyMetadata(typeof (RibbonTab)));
        }

        #endregion

        #region Dependency Properties

        public string Text {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof (string), typeof (RibbonTab), new PropertyMetadata(string.Empty));


        public Uri ContentSource {
            get { return (Uri) GetValue(ContentSourceProperty); }
            set { SetValue(ContentSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContentSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentSourceProperty =
            DependencyProperty.Register("ContentSource", typeof (Uri), typeof (RibbonTab),
                new PropertyMetadata(new Uri("about:blank")));

        public bool IsSelected {
            get { return (bool) GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof (bool), typeof (RibbonTab), new PropertyMetadata(false));

        #endregion
    }
}