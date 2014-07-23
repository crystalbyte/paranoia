using System;
using System.Collections;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Controls.Primitives;

namespace Crystalbyte.Paranoia.UI {
    [TemplatePart(Name = SuggestionHostPartName, Type = typeof(Popup))]
    [TemplatePart(Name = SuggestionListPartName, Type = typeof(ListView))]
    public sealed class AutoCompleteBox : TextBox {

        #region Xaml Support

        private const string SuggestionHostPartName = "PART_SuggestionHost";
        private const string SuggestionListPartName = "PART_SuggestionList";

        #endregion

        #region Private Fields

        private Popup _suggestionHost;
        private ListView _suggestionList;

        #endregion

        #region Construction

        static AutoCompleteBox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteBox),
                new FrameworkPropertyMetadata(typeof(AutoCompleteBox)));
        }

        #endregion

        #region Public Events

        public event EventHandler ItemsSourceRequested;

        private void OnItemsSourceRequested() {
            var handler = ItemsSourceRequested;
            if (handler != null) 
                handler(this, EventArgs.Empty);
        }

        #endregion

        #region Dependency Properties

        public object ItemsSource {
            get { return GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(object), typeof(AutoCompleteBox), new PropertyMetadata(null));

        public DataTemplate ItemTemplate {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(AutoCompleteBox), new PropertyMetadata(null));

        public ItemsPanelTemplate ItemsPanel {
            get { return (ItemsPanelTemplate)GetValue(ItemsPanelProperty); }
            set { SetValue(ItemsPanelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsPanel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsPanelProperty =
            DependencyProperty.Register("ItemsPanel", typeof(ItemsPanelTemplate), typeof(AutoCompleteBox), new PropertyMetadata(null));

        #endregion

        #region Class Overrides

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                action => TextChanged += action,
                action => TextChanged -= action)
            .Throttle(TimeSpan.FromMilliseconds(250))
            .ObserveOn(SynchronizationContext.Current)
            .Select(x => ((TextBox)x.Sender).Text)
            .Subscribe(OnTextChangeConfirmed);
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            _suggestionHost = (Popup) Template.FindName(SuggestionHostPartName, this);
            _suggestionList = (ListView)Template.FindName(SuggestionListPartName, this);
        }

        #endregion

        #region Methods

        private void OnTextChangeConfirmed(string text) {
            OnItemsSourceRequested();

            var source = ItemsSource as ICollection;
            if (source != null && source.Count > 0) {
                _suggestionHost.IsOpen = true;
            }
            else {
                _suggestionHost.IsOpen = false;
            }
        }

        #endregion
    }
}
