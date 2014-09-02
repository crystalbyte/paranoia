#region Using directives

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Awesomium.Core;
using Awesomium.Windows.Controls;
using Crystalbyte.Paranoia.Properties;

#endregion

namespace Crystalbyte.Paranoia.UI {
    [TemplatePart(Name = WebControlPartName, Type = typeof(WebControl))]
    public class HtmlControl : Control {

        #region Xaml Support

        private const string WebControlPartName = "PART_WebControl";

        #endregion

        #region Private Fields

        private bool _disposed;
        private WebControl _webControl;

        #endregion

        #region Construction

        static HtmlControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HtmlControl),
                new FrameworkPropertyMetadata(typeof(HtmlControl)));
        }

        public HtmlControl() {
            HtmlFont = new FontFamily(Settings.Default.HtmlDefaultFontFamily);
            HtmlFontSize = Settings.Default.HtmlDefaultFontSize;

            GotKeyboardFocus += OnGotKeyboardFocus;
            IsKeyboardFocusWithinChanged += (sender, e) => Debug.WriteLine(Keyboard.FocusedElement);

            if (!DesignerProperties.GetIsInDesignMode(this)) {
                Source = WebCore.Configuration.HomeURL.ToString();
            }
            WebCore.Initialized += (sender, e) => {
                Dispatcher.Invoke(() => {
                    var resourceInterceptor = new ResourceInterceptor();
                    resourceInterceptor.SetCurrentMessage();
                    WebCore.ResourceInterceptor = resourceInterceptor;
                });
            };
        }

        private void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            var presenter = e.NewFocus as WebViewPresenter;
            if (presenter == null)
                return;

            FocusEntryParagraph();
        }

        #endregion

        #region Destruction

        ~HtmlControl() {
            Dispose(false);
        }

        #endregion

        #region Dependency Properties

        public float Zoom {
            get { return (float)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Zoom.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(float), typeof(HtmlControl), new PropertyMetadata(1.45f));

        public bool IsTransparent {
            get { return (bool)GetValue(IsTransparentProperty); }
            set { SetValue(IsTransparentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTransparent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTransparentProperty =
            DependencyProperty.Register("IsTransparent", typeof(bool), typeof(HtmlControl),
                new PropertyMetadata(false));

        // This will be set to the target URL, when this window does not
        // host a created child view. The WebControl, is bound to this property.
        public string Source {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Identifies the <see cref="Source"/> dependency property.
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source",
                typeof(string), typeof(HtmlControl),
                new FrameworkPropertyMetadata(string.Empty));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(HtmlControl), new PropertyMetadata(false));

        public FontFamily HtmlFont {
            get { return (FontFamily)GetValue(HtmlFontProperty); }
            set { SetValue(HtmlFontProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedFont.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HtmlFontProperty =
            DependencyProperty.Register("HtmlFont", typeof(FontFamily), typeof(HtmlControl),
                new PropertyMetadata(null));

        public int HtmlFontSize {
            get { return (int)GetValue(HtmlFontSizeProperty); }
            set { SetValue(HtmlFontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HtmlFontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HtmlFontSizeProperty =
            DependencyProperty.Register("HtmlFontSize", typeof(int), typeof(HtmlControl), new PropertyMetadata(0));

        #endregion

        #region Class Overrides

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            if (_webControl != null) {
                _webControl.ShowCreatedWebView -= OnWebControlShowCreatedWebView;
                _webControl.WindowClose -= OnWebControlWindowClose;
            }

            _webControl = (WebControl)Template.FindName(WebControlPartName, this);
            _webControl.ShowCreatedWebView += OnWebControlShowCreatedWebView;
            _webControl.WindowClose += OnWebControlWindowClose;
        }

        private void OnWebControlWindowClose(object sender, WindowCloseEventArgs e) { }

        private void OnWebControlShowCreatedWebView(object sender, ShowCreatedWebViewEventArgs e) {

        }

        #endregion

        #region Methods

        public string GetDocument() {
            return _webControl.HTML;
        }

        private void FocusEntryParagraph() {
            if (!_webControl.IsDocumentReady)
                return;

            const string script = ";";
            _webControl.ExecuteJavascript(script);
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            if (_disposed) {
                return;
            }

            if (disposing) {
                // Nothing managed to dispose here.
            }

            if (_webControl != null) {
                _webControl.Dispose();
            }

            _disposed = true;
        }

        #endregion

        internal string GetEditorDocument() {
            try {
                var html = _webControl.ExecuteJavascriptWithResult("getEditorHtml()");
                return html;
            } catch (Exception ex) {
                Debug.WriteLine("something went wrong\n" + ex);
                return string.Empty;
            }

        }
    }
}