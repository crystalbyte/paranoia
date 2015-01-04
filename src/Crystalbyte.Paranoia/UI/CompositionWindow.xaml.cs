using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for CompositionWindow.xaml
    /// </summary>
    public partial class CompositionWindow {
        public CompositionWindow() {
            InitializeComponent();
            
            CommandBindings.Add(new CommandBinding(WindowCommands.Maximize, OnMaximize));
            CommandBindings.Add(new CommandBinding(WindowCommands.Minimize, OnMinimize));
            CommandBindings.Add(new CommandBinding(WindowCommands.RestoreDown, OnRestoreDown));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, OnHelp));
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
