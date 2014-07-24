using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using Crystalbyte.Paranoia.UI.Commands;

namespace Crystalbyte.Paranoia.UI {
    [TemplatePart(Name = AutoCompletePopupPartName, Type = typeof(Popup))]
    [TemplatePart(Name = AutoCompleteHostPartName, Type = typeof(ListView))]
    public sealed class AutoCompleteBox : RichTextBox {

        #region Xaml Support

        private const string AutoCompletePopupPartName = "PART_AutoCompletePopup";
        private const string AutoCompleteHostPartName = "PART_AutoCompleteHost";

        #endregion

        #region Private Fields

        private Popup _autoCompletePopup;
        private ListView _autoCompleteHost;
        private readonly ICommand _selectCommand;
        private readonly List<ITokenMatcher> _tokenMatchers;
        private bool _suppressRecognition;

        #endregion

        #region Construction

        static AutoCompleteBox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteBox),
                new FrameworkPropertyMetadata(typeof(AutoCompleteBox)));
        }

        public AutoCompleteBox() {
            _tokenMatchers = new List<ITokenMatcher> { new MailAddressTokenMatcher() };
            _selectCommand = new RelayCommand(OnSelectCommandExecuted);

            CommandBindings.Add(new CommandBinding(AutoCompleteBoxCommands.Delete, DeleteToken));
            CommandBindings.Add(new CommandBinding(AutoCompleteBoxCommands.AutoComplete, AutoComplete));
            CommandBindings.Add(new CommandBinding(AutoCompleteBoxCommands.CloseAutoComplete, CloseAutoComplete));
        }

   

        #endregion

        #region Public Events

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

        public ICommand SelectCommand {
            get { return _selectCommand; }
        }

        public ICollection<ITokenMatcher> TokenMatchers {
            get { return _tokenMatchers; }
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

        private void OnTextChanged(object sender, TextChangedEventArgs e) {
            if (_suppressRecognition) {
                return;
            }

            var text = CaretPosition.GetTextInRun(LogicalDirection.Backward);
            var success = _tokenMatchers.Any(x => x.IsMatch(text));
            if (!success) {
                return;
            }

            CreateToken(text);
        }

        private void CreateToken(string text) {
            var container = CreateTokenContainerFromString(text);
            var paragraph = CaretPosition.Paragraph;
            if (paragraph == null) {
                throw new Exception("?");
            }
            _suppressRecognition = true;
            paragraph.Inlines.Add(container);
            _suppressRecognition = false;
        }

        private InlineUIContainer CreateTokenContainerFromString(string value) {
            return new InlineUIContainer(new ContentPresenter {
                Content = value,
                ContentTemplate = StringTokenTemplate
            });
        }

        private InlineUIContainer CreateTokenContainerFromItem(object value) {
            return new InlineUIContainer(new ContentPresenter {
                Content = value,
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

        private void CloseAutoComplete(object sender, ExecutedRoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void AutoComplete(object sender, ExecutedRoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void DeleteToken(object sender, ExecutedRoutedEventArgs e) {
            throw new NotImplementedException();
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

        private void OnSelectCommandExecuted(object obj) {
            CommitSelection();
        }

        private void CommitSelection() {
            var container = GetFirstItem();

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

        #endregion
    }
}
