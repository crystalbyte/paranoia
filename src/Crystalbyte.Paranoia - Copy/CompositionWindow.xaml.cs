using System;
using System.Windows;
using System.Windows.Navigation;
using Crystalbyte.Paranoia.UI;

namespace Crystalbyte.Paranoia {
    /// <summary>
    /// Interaction logic for CompositionWindow.xaml
    /// </summary>
    public partial class CompositionWindow {
        public CompositionWindow() {
            InitializeComponent();
        }

        public Uri Source {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(Uri), typeof(CompositionWindow), new PropertyMetadata(null));

        private void OnCloseButtonClick(object sender, RoutedEventArgs e) {
            Close();
        }

        private void OnFrameNavigated(object sender, NavigationEventArgs e) {
            var page = e.Content as INavigationAware;
            if (page != null) {
                page.OnNavigated(e);
            }
        }
    }
}
