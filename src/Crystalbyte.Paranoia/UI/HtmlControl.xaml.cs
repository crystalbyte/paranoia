#region Using directives

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Awesomium.Core;
using Awesomium.Windows.Controls;
using Crystalbyte.Paranoia.Properties;
using System.Text.RegularExpressions;
using System.IO;
using dotless.Core;

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

        protected override void OnPreviewKeyDown(KeyEventArgs e) {
            base.OnPreviewKeyDown(e);

            if (e.Key != Key.V || (Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                return;

            e.Handled = true;
            PasteClipboardContent();
        }

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

        public static string CustomCss {
            get {

                if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) {
                    return "body {}";
                }
                var uri = new Uri("/Resources/inspector.default.less", UriKind.Relative);
                var info = Application.GetResourceStream(uri);
                if (info == null) {
                    var error = string.Format(Properties.Resources.ResourceNotFoundException, uri, typeof(App).Assembly.FullName);
                    throw new Exception(error);
                }


                string less;
                const string pattern = "%.+?%";
                using (var reader = new StreamReader(info.Stream)) {
                    var text = reader.ReadToEnd();
                    less = Regex.Replace(text, pattern, m => {
                        var key = m.Value.Trim('%');
                        var resource = App.ThemeDictionary[key];
                        if (resource == null) {
                            return "fuchsia";
                        }
                        // Drop the alpha channel.
                        return string.Format("#{0}",
                            resource.ToString().Substring(3));
                    });
                }

                return Less.Parse(less);
            }
        }

        internal string GetEditorDocument() {
            try {
                var html = _webControl.ExecuteJavascriptWithResult("getEditorHtml()");
                return html;
            } catch (Exception ex) {
                Debug.WriteLine("something went wrong\n" + ex);
                return string.Empty;
            }
        }

        internal void InsertHtmlAtCurrentPosition(string html) {
            JSObject window = _webControl.ExecuteJavascriptWithResult("window");

            using (window) {
                window.Invoke("pasteHtmlAtCurrentPosition", html);
            }
        }

        internal void InsertPlaneAtCurrentPosition(string planeText) {
            _webControl.ExecuteJavascript(string.Format("pastePlaneAtCurrentPosition({0})", planeText));
        }

        #region PasteHandler

        private void PasteClipboardContent() {
            var data = Clipboard.GetDataObject();
            if (data == null)
                return;

            // Debug stuff
            // ReSharper disable once UnusedVariable
            var formats = data.GetFormats();

            var image = data.GetData("System.Drawing.Bitmap") as Bitmap;
            if (image != null) {
                var file = Path.GetTempFileName() + ".jpg";
                image.Save(file);
                InsertHtmlAtCurrentPosition(string.Format("<img width=480 src=\"asset://tempImage/{0}\"></img>", file));
                return;
            }

            var html = (string)data.GetData(DataFormats.Html);
            if (html != null) {
                var htmlRegex = new Regex("<html.*?</html>",
                RegexOptions.Singleline);
                var temp = htmlRegex.Match(html).Value;

                var conditionRegex = new Regex(@"<!--\[if.*?<!\[endif]-->", RegexOptions.Singleline);
                const string imageTagRegexPattern = "<img.*?>(</img>){0,1}";
                const string srcPrepRegexPatter = "src=\".*?\"";
                temp = conditionRegex.Replace(temp, string.Empty);
                temp = temp.Replace("<![if !vml]>", string.Empty)
                    .Replace("<![endif]>", string.Empty);
                var imageTagMatches = Regex.Matches(temp, imageTagRegexPattern, RegexOptions.Singleline | RegexOptions.Compiled);
                foreach (Match match in imageTagMatches) {
                    var originalSrcFile = Regex.Match(match.Value, srcPrepRegexPatter).Value;
                    var srcFile = originalSrcFile.Replace("src=\"", string.Empty).Replace("\"", string.Empty).Replace("file:///", string.Empty);

                    if (!new Uri(srcFile).IsFile)
                        continue;

                    if (!File.Exists(srcFile))
                        throw new Exception("701");

                    temp = temp.Replace(originalSrcFile, string.Format("src=\"asset://tempImage/{0}\"", srcFile));
                }

                html = temp;
                InsertHtmlAtCurrentPosition(html);
                return;
            }

            var planeText = (string)data.GetData(DataFormats.Text);
            if (planeText == null)
                return;

            InsertPlaneAtCurrentPosition(planeText);
        }

        #endregion
    }
}