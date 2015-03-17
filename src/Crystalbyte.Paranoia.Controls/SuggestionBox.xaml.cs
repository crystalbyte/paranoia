#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia.Controls
// 
// Crystalbyte.Paranoia.Controls is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using Directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

#endregion

namespace Crystalbyte.Paranoia.UI {
    [TemplatePart(Name = PopupPartName, Type = typeof (Popup))]
    [TemplatePart(Name = HostPartName, Type = typeof (ListView))]
    public sealed class SuggestionBox : RichTextBox {
        #region Xaml Support

        private const string PopupPartName = "PART_Popup";
        private const string HostPartName = "PART_ItemsHost";

        #endregion

        #region Private Fields

        private Popup _popup;
        private ListView _itemsHost;
        private bool _suppressRecognition;
        private readonly List<ITokenMatcher> _tokenMatchers;
        private readonly ObservableCollection<object> _selectedValues;

        #endregion

        #region Construction

        static SuggestionBox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (SuggestionBox),
                new FrameworkPropertyMetadata(typeof (SuggestionBox)));
        }

        public SuggestionBox() {
            _selectedValues = new ObservableCollection<object>();
            _tokenMatchers = new List<ITokenMatcher> {new MailAddressTokenMatcher()};

            CommandBindings.Add(new CommandBinding(SuggestionBoxCommands.Select, OnSelectContact));
            SelectedValues = _selectedValues;
        }

        #endregion

        #region Public Events

        public event EventHandler SelectedValuesChanged;

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

        private bool IsStyleApplied {
            get { return _popup != null; }
        }

        #endregion

        #region Dependency Properties

        public Brush AccentBrush {
            get { return (Brush) GetValue(AccentBrushProperty); }
            set { SetValue(AccentBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AccentBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AccentBrushProperty =
            DependencyProperty.Register("AccentBrush", typeof (Brush), typeof (SuggestionBox),
                new PropertyMetadata(null));

        public bool IsWatermarkVisible {
            get { return (bool) GetValue(IsWatermarkVisibleProperty); }
            set { SetValue(IsWatermarkVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsWatermarkVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsWatermarkVisibleProperty =
            DependencyProperty.Register("IsWatermarkVisible", typeof (bool), typeof (SuggestionBox),
                new PropertyMetadata(false));


        public object Watermark {
            get { return GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof (object), typeof (SuggestionBox), new PropertyMetadata(null));


        public DataTemplate WatermarkTemplate {
            get { return (DataTemplate) GetValue(WatermarkTemplateProperty); }
            set { SetValue(WatermarkTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WatermarkTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WatermarkTemplateProperty =
            DependencyProperty.Register("WatermarkTemplate", typeof (DataTemplate), typeof (SuggestionBox),
                new PropertyMetadata(null));

        public IEnumerable<object> SelectedValues {
            get { return (IEnumerable<object>) GetValue(SelectedValuesProperty); }
            set { SetValue(SelectedValuesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedValues.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedValuesProperty =
            DependencyProperty.Register("SelectedValues", typeof (IEnumerable<object>), typeof (SuggestionBox),
                new PropertyMetadata(null));

        public Style ItemContainerStyle {
            get { return (Style) GetValue(ItemContainerStyleProperty); }
            set { SetValue(ItemContainerStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemContainerStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register("ItemContainerStyle", typeof (Style), typeof (SuggestionBox),
                new PropertyMetadata(null));


        public object ItemsSource {
            get { return GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof (object), typeof (SuggestionBox),
                new PropertyMetadata(OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var box = (SuggestionBox) d;
            box.OnItemsSourceChanged(e.NewValue, e.OldValue);
        }

        public DataTemplate ItemTemplate {
            get { return (DataTemplate) GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof (DataTemplate), typeof (SuggestionBox),
                new PropertyMetadata(null));

        public ItemsPanelTemplate ItemsPanel {
            get { return (ItemsPanelTemplate) GetValue(ItemsPanelProperty); }
            set { SetValue(ItemsPanelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsPanel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsPanelProperty =
            DependencyProperty.Register("ItemsPanel", typeof (ItemsPanelTemplate), typeof (SuggestionBox),
                new PropertyMetadata(null));

        public DataTemplate TokenTemplate {
            get { return (DataTemplate) GetValue(TokenTemplateProperty); }
            set { SetValue(TokenTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TokenTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TokenTemplateProperty =
            DependencyProperty.Register("TokenTemplate", typeof (DataTemplate), typeof (SuggestionBox),
                new PropertyMetadata(null));

        public DataTemplate StringTokenTemplate {
            get { return (DataTemplate) GetValue(StringTokenTemplateProperty); }
            set { SetValue(StringTokenTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StringTokenTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StringTokenTemplateProperty =
            DependencyProperty.Register("StringTokenTemplate", typeof (DataTemplate), typeof (SuggestionBox),
                new PropertyMetadata(null));

        #endregion

        #region Class Overrides

        protected override void OnPreviewKeyDown(KeyEventArgs e) {
            base.OnPreviewKeyDown(e);

            var container = GetFirstItem();
            if (container == null) {
                return;
            }

            if (e.Key == Key.Up && _popup.IsOpen && container.IsSelected) {
                e.Handled = true;
                container.IsSelected = false;
                Focus();
                return;
            }

            if (e.Key != Key.Tab || !_popup.IsOpen || !IsFocused)
                return;

            e.Handled = true;
            container.IsSelected = true;
            CommitSelection();
            Close();
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e) {
            base.OnPreviewKeyUp(e);

            if (e.Key == Key.Down && _popup.IsOpen && IsFocused) {
                e.Handled = true;
                SelectFirstElement();
                return;
            }

            if (e.Key != Key.Escape || !_popup.IsOpen)
                return;

            e.Handled = true;
            Close();
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e) {
            base.OnLostKeyboardFocus(e);

            InvalidateWatermark();
        }

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            if (DesignerProperties.GetIsInDesignMode(this)) {
                return;
            }

            Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                action => TextChanged += action,
                action => TextChanged -= action)
                .Where(x => !DesignerProperties.GetIsInDesignMode(this))
                .Where(x => IsStyleApplied)
                .Where(x => {
                           var text = ((SuggestionBox) x.Sender).Text;
                           return !string.IsNullOrWhiteSpace(text) && text.Length > 1;
                       })
                .Throttle(TimeSpan.FromMilliseconds(30))
                .Select(x => ((SuggestionBox) x.Sender).Text)
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
            return new InlineUIContainer(presenter)
            {
                BaselineAlignment = BaselineAlignment.Center
            };
        }

        private InlineUIContainer CreateTokenContainerFromString(string value) {
            return CreateContainer(new ContentPresenter
            {
                Content = value,
                DataContext = value,
                ContentTemplate = StringTokenTemplate
            });
        }

        private InlineUIContainer CreateTokenContainerFromItem(object value) {
            return CreateContainer(new ContentPresenter
            {
                Content = value,
                DataContext = value,
                ContentTemplate = TokenTemplate
            });
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            if (DesignerProperties.GetIsInDesignMode(this)) {
                return;
            }

            _popup = (Popup) Template.FindName(PopupPartName, this);
            _itemsHost = (ListView) Template.FindName(HostPartName, this);
            _itemsHost.InputBindings.Add(new KeyBinding(SuggestionBoxCommands.Select, Key.Enter, ModifierKeys.None));
            _itemsHost.InputBindings.Add(new KeyBinding(SuggestionBoxCommands.Select, Key.Tab, ModifierKeys.None));

            InvalidateWatermark();
        }

        #endregion

        #region Methods

        public void Preset(params string[] strings) {
            foreach (var token in strings.Select(CreateTokenContainerFromString)) {
                AppendContainer(token);
            }
        }

        public void Preset(IEnumerable<object> items) {
            foreach (var token in items.Select(CreateTokenContainerFromItem)) {
                AppendContainer(token);
            }
        }

        private void OnItemsSourceChanged(object newSource, object oldSource) {
            var oldCollection = oldSource as INotifyCollectionChanged;
            if (oldCollection != null) {
                oldCollection.CollectionChanged -= OnItemsSourceCollectionChanged;
            }

            var newCollection = newSource as INotifyCollectionChanged;
            if (newCollection != null) {
                newCollection.CollectionChanged += OnItemsSourceCollectionChanged;
            }

            CheckPopupVisibility();
        }

        private void CheckPopupVisibility() {
            var source = ItemsSource as ICollection;
            if (source != null && source.Count > 0) {
                _popup.IsOpen = true;
            }
            else {
                _popup.IsOpen = false;
            }
        }

        private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            CheckPopupVisibility();
        }

        private ListViewItem GetFirstItem() {
            var source = ItemsSource as IList;
            if (source == null || source.Count <= 0)
                return null;

            var item = source[0];
            if (item == null)
                return null;

            return _itemsHost.ItemContainerGenerator
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
            _popup.IsOpen = false;
            Focus();
        }

        private void OnSelectContact(object sender, EventArgs e) {
            _popup.IsOpen = false;
            CommitSelection();
            Focus();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e) {
            RecognizeMatches();
            UpdateSelectedValues();
            InvalidateWatermark();
        }

        private void InvalidateWatermark() {
            IsWatermarkVisible = string.IsNullOrEmpty(Text) && _selectedValues.Count == 0;
        }

        private void UpdateSelectedValues() {
            var paragraph = CaretPosition.Paragraph;
            if (paragraph == null) {
                return;
            }

            var objects = paragraph.Inlines
                .OfType<InlineUIContainer>()
                .Select(x => ((ContentPresenter) x.Child).Content);

            _selectedValues.Clear();
            _selectedValues.AddRange(objects);

            OnSelectedValuesChanged();
        }

        private void CommitSelection() {
            var item = _itemsHost.SelectedItem;
            var token = CreateTokenContainerFromItem(item);
            AppendContainer(token);
        }

        private void OnTextChangeConfirmed(string text) {
            var e = new ItemsSourceRequestedEventArgs(text);
            OnItemsSourceRequested(e);
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