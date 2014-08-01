using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

namespace Crystalbyte.Paranoia.UI {
    [TemplatePart(Name = AutoCompletePopupPartName, Type = typeof(Popup))]
    [TemplatePart(Name = AutoCompleteHostPartName, Type = typeof(ListView))]
    public sealed class AutoCompleteBox : RichTextBox {

        #region Xaml Support

        private const string AutoCompletePopupPartName = "PART_AutoCompletePopup";
        private const string AutoCompleteHostPartName = "PART_AutoCompleteHost";

        #endregion

        #region Private Fields

        private bool _suppressRecognition;
        private Popup _autoCompletePopup;
        private ListView _autoCompleteHost;
        private readonly List<ITokenMatcher> _tokenMatchers;
        private readonly ObservableCollection<object> _selectedValues;

        #endregion

        #region Construction

        static AutoCompleteBox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteBox),
                new FrameworkPropertyMetadata(typeof(AutoCompleteBox)));
        }

        public AutoCompleteBox() {
            _selectedValues = new ObservableCollection<object>();
            _tokenMatchers = new List<ITokenMatcher> { new MailAddressTokenMatcher() };

            CommandBindings.Add(new CommandBinding(AutoCompleteBoxCommands.Select, OnSelectContact));
            SelectedValues = _selectedValues;
            
        }

        #endregion

        #region Public Events

        public new event EventHandler SelectedValuesChanged;

        private void OnSelectedValuesChanged() {
            var handler = SelectedValuesChanged;
            if (handler != null) 
                handler(this, EventArgs.Empty);
        }

        public event EventHandler<ItemsSourceRequestedEventArgs> ItemsSourceRequested;

        private void OnItemsSourceRequested(ItemsSourceRequestedEventArgs e) {
            var handler = ItemsSourceRequested;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        #region Property Declarations

        public string Text {
            get { return CaretPosition.GetTextInRun(LogicalDirection.Backward); }
        }

        public ICollection<ITokenMatcher> TokenMatchers {
            get { return _tokenMatchers; }
        }

        #endregion

        #region Dependency Properties

        public IEnumerable<object> SelectedValues {
            get { return (IEnumerable<object>)GetValue(SelectedValuesProperty); }
            set { SetValue(SelectedValuesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedValues.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedValuesProperty =
            DependencyProperty.Register("SelectedValues", typeof(IEnumerable<object>), typeof(AutoCompleteBox), new PropertyMetadata(null));


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

        public DataTemplate TokenTemplate {
            get { return (DataTemplate)GetValue(TokenTemplateProperty); }
            set { SetValue(TokenTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TokenTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TokenTemplateProperty =
            DependencyProperty.Register("TokenTemplate", typeof(DataTemplate), typeof(AutoCompleteBox), new PropertyMetadata(null));

        public DataTemplate StringTokenTemplate {
            get { return (DataTemplate)GetValue(StringTokenTemplateProperty); }
            set { SetValue(StringTokenTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StringTokenTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StringTokenTemplateProperty =
            DependencyProperty.Register("StringTokenTemplate", typeof(DataTemplate), typeof(AutoCompleteBox), new PropertyMetadata(null));

        #endregion

        #region Class Overrides

        protected override void OnPreviewKeyUp(KeyEventArgs e) {
            base.OnPreviewKeyUp(e);

            if (e.Key == Key.Down && _autoCompletePopup.IsOpen) {
                SelectFirstElement();
                e.Handled = true;
                return;
            }

            var container = GetFirstItem();
            if (e.Key != Key.Up
                || !_autoCompletePopup.IsOpen
                || !container.IsSelected)
                return;

            FocusInputControl(container);
            e.Handled = true;
        }

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                action => TextChanged += action,
                action => TextChanged -= action)
            .Throttle(TimeSpan.FromMilliseconds(250))
            .ObserveOn(SynchronizationContext.Current)
            .Select(x => ((AutoCompleteBox)x.Sender).Text)
            .Subscribe(OnTextChangeConfirmed);

            TextChanged += OnTextChanged;
        }

        private void AppendContainer(Inline container) {
            _suppressRecognition = true;

            CaretPosition = CaretPosition.DocumentEnd;
            var length = CaretPosition.GetTextRunLength(LogicalDirection.Backward);
            CaretPosition = CaretPosition.GetPositionAtOffset(-length);

            if (CaretPosition != null) {
                CaretPosition.DeleteTextInRun(length);

                var paragraph = CaretPosition.Paragraph;
                if (paragraph != null) {
                    paragraph.Inlines.Add(container);
                }

                CaretPosition = CaretPosition.DocumentEnd;
            }

            _suppressRecognition = false;
        }

        private static InlineUIContainer CreateContainer(UIElement presenter) {
            return new InlineUIContainer(presenter) {
                BaselineAlignment = BaselineAlignment.Center
            };
        }

        private InlineUIContainer CreateTokenContainerFromString(string value) {
            return CreateContainer(new ContentPresenter {
                Content = value,
                DataContext = value,
                ContentTemplate = StringTokenTemplate
            });
        }

        private InlineUIContainer CreateTokenContainerFromItem(object value) {
            return CreateContainer(new ContentPresenter {
                Content = value,
                DataContext = value,
                ContentTemplate = TokenTemplate
            });
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            _autoCompletePopup = (Popup)Template.FindName(AutoCompletePopupPartName, this);
            _autoCompleteHost = (ListView)Template.FindName(AutoCompleteHostPartName, this);
        }

        #endregion

        #region Methods

        public void Preset(params string[] strings) {
            foreach (var token in strings.Select(CreateTokenContainerFromString)) {
                AppendContainer(token);
            }
        }

        public void Preset(params object[] items) {
            foreach (var token in items.Select(CreateTokenContainerFromItem)) {
                AppendContainer(token);
            }
        }

        private void FocusInputControl(ListBoxItem item) {
            item.IsSelected = false;
            Focus();
        }

        private ListViewItem GetFirstItem() {
            var source = ItemsSource as IList;
            if (source == null || source.Count <= 0)
                return null;

            var item = source[0];
            if (item == null)
                return null;

            return _autoCompleteHost.ItemContainerGenerator
                .ContainerFromItem(item) as ListViewItem;
        }

        private void SelectFirstElement() {
            var container = GetFirstItem();
            if (container == null)
                return;

            container.Focus();
            container.IsSelected = true;
        }

        internal void Close() {
            _autoCompletePopup.IsOpen = false;
        }

        private void OnSelectContact(object sender, EventArgs e) {
            _autoCompletePopup.IsOpen = false;
            CommitSelection();
            Focus();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e) {
            RecognizeMatches();
            UpdateSelectedValues();
        }

        private void UpdateSelectedValues() {
            var paragraph = CaretPosition.Paragraph;
            if (paragraph == null) {
                return;
            }

            var objects = paragraph.Inlines
                .OfType<InlineUIContainer>()
                .Select(x => ((ContentPresenter)x.Child).Content);

            _selectedValues.Clear();
            _selectedValues.AddRange(objects);

            OnSelectedValuesChanged();
        }

        private void CommitSelection() {
            var item = _autoCompleteHost.SelectedItem;
            var token = CreateTokenContainerFromItem(item);
            AppendContainer(token);
        }

        private void OnTextChangeConfirmed(string text) {
            OnItemsSourceRequested(new ItemsSourceRequestedEventArgs(text));

            var source = ItemsSource as ICollection;
            if (source != null && source.Count > 0) {
                _autoCompletePopup.IsOpen = true;
            } else {
                _autoCompletePopup.IsOpen = false;
            }
        }

        private void RecognizeMatches() {
            if (_suppressRecognition) {
                return;
            }

            var text = CaretPosition.GetTextInRun(LogicalDirection.Backward);
            var match = string.Empty;
            var success = _tokenMatchers.Any(x => x.TryMatch(text, out match));

            if (!success)
                return;

            var container = CreateTokenContainerFromString(match);
            AppendContainer(container);
        }

        #endregion
    }
}
