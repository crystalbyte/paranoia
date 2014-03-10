#region Using directives

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

#endregion

namespace Crystalbyte.Paranoia.UI {
    [ContentProperty("Tabs")]
    public class Ribbon : Control {
        #region Construction

        static Ribbon() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Ribbon), new FrameworkPropertyMetadata(typeof(Ribbon)));
        }

        #endregion

        #region Dependency Properties

        public string AppMenuText {
            get { return (string)GetValue(AppMenuTextProperty); }
            set { SetValue(AppMenuTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AppMenuButtonContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AppMenuTextProperty =
            DependencyProperty.Register("AppMenuText", typeof(string), typeof(Ribbon),
                new PropertyMetadata(string.Empty));

        public ObservableCollection<RibbonTab> Tabs {
            get { return (ObservableCollection<RibbonTab>)GetValue(TabsProperty); }
            set { SetValue(TabsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Tabs.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TabsProperty =
            DependencyProperty.Register("Tabs", typeof(ObservableCollection<RibbonTab>), typeof(Ribbon),
                new PropertyMetadata(new ObservableCollection<RibbonTab>(), OnTabsPropertyChanged));

        private static void OnTabsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var ribbon = (Ribbon)d;
            var old = e.OldValue as ObservableCollection<RibbonTab>;
            if (old != null) {
                ribbon.Detach(old);
            }

            var @new = e.NewValue as ObservableCollection<RibbonTab>;
            if (@new != null) {
                ribbon.Attach(@new);
            }
        }

        #endregion

        private void Attach(INotifyCollectionChanged @new) {
            @new.CollectionChanged += OnCollectionChanged;
        }

        private void Detach(INotifyCollectionChanged old) {
            old.CollectionChanged -= OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { }
    }
}