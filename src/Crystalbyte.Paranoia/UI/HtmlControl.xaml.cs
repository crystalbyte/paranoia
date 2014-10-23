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
using System.Text.RegularExpressions;
using System.IO;
using NLog;

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

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        static HtmlControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HtmlControl),
                new FrameworkPropertyMetadata(typeof(HtmlControl)));
        }

        public HtmlControl() {
            IsKeyboardFocusWithinChanged += (sender, e) => Debug.WriteLine(Keyboard.FocusedElement);

            //WebCore.CreatedView += WebCore_CreatedView;

            if (!DesignerProperties.GetIsInDesignMode(this)) {
                Source = WebCore.Configuration.HomeURL.ToString();
            }
        }

        //void WebCore_CreatedView(object sender, CreatedViewEventArgs e) {
        //    e.NewView.NativeViewInitialized += NewView_NativeViewInitialized;
        //}

        //void NewView_NativeViewInitialized(object sender, WebViewEventArgs e) {
        //    var navigationInterceptor = ((IServiceProvider)sender).GetService(typeof(INavigationInterceptor)) as INavigationInterceptor;
        //    navigationInterceptor.ImplicitRule = NavigationRule.Deny;
        //    navigationInterceptor.BeginLoadingFrame += navigationInterceptor_BeginLoadingFrame;
        //    navigationInterceptor.BeginNavigation += navigationInterceptor_BeginNavigation;
        //}

        //void navigationInterceptor_BeginLoadingFrame(object sender, BeginLoadingFrameEventArgs e) {
        //    throw new NotImplementedException();
        //}

        //void navigationInterceptor_BeginNavigation(object sender, NavigationEventArgs e) {
        //    var a = e.Url;
        //}

        #endregion

        #region Destruction

        ~HtmlControl() {
            Dispose(false);
        }

        #endregion

        #region Event Declarations

        public event EventHandler<PrintCompleteEventArgs> PrintSourcesCreated;

        protected virtual void OnPrintSourcesCreated(PrintCompleteEventArgs e) {
            var handler = PrintSourcesCreated;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region Dependency Properties

        public WebSession WebSession {
            get { return (WebSession)GetValue(WebSessionProperty); }
            set { SetValue(WebSessionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WebSession.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WebSessionProperty =
            DependencyProperty.Register("WebSession", typeof(WebSession), typeof(HtmlControl), new PropertyMetadata(null));

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
                _webControl.GotKeyboardFocus -= OnGotKeyboardFocus;
                _webControl.ShowCreatedWebView -= OnWebControlShowCreatedWebView;
                _webControl.WindowClose -= OnWebControlWindowClose;
            }

            _webControl = (WebControl)Template.FindName(WebControlPartName, this);
            _webControl.GotKeyboardFocus += OnGotKeyboardFocus;
            _webControl.ShowCreatedWebView += OnWebControlShowCreatedWebView;
            _webControl.WindowClose += OnWebControlWindowClose;
        }

        private void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (_webControl.IsDocumentReady) {
                _webControl.ExecuteJavascript("focusEditor();");
            }
        }

        private void OnWebControlWindowClose(object sender, WindowCloseEventArgs e) {

        }

        private void OnWebControlShowCreatedWebView(object sender, ShowCreatedWebViewEventArgs e) {

        }

        #endregion

        #region Methods

        public string GetDocument() {
            return _webControl.HTML;
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

        internal string GetHtmlContent() {
            const string function = "Crystalbyte.Paranoia.getContent();";
            var html = _webControl.ExecuteJavascriptWithResult(function);
            return html;
        }

        internal void InsertHtml(string html) {
            JSObject module = _webControl.ExecuteJavascriptWithResult("Crystalbyte.Paranoia");
            using (module) {
                const string function = "insertHtml";
                module.Invoke(function, html);
            }
        }

        internal void InsertText(string text) {
            JSObject module = _webControl.ExecuteJavascriptWithResult("Crystalbyte.Paranoia");
            using (module) {
                const string function = "insertText";
                module.Invoke(function, text);
            }
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
                InsertHtml(string.Format("<img width=480 src=\"asset://tempImage/{0}\"></img>", file));
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

                    Uri uri;
                    if (!Uri.TryCreate(srcFile, UriKind.Absolute, out uri) || !uri.IsFile)
                        continue;


                    if (!File.Exists(srcFile))
                        throw new Exception("701");

                    temp = temp.Replace(originalSrcFile, string.Format("src=\"asset://tempImage/{0}\"", srcFile));
                }

                html = temp;
                InsertHtml(html);
                return;
            }

            var planeText = (string)data.GetData(DataFormats.Text);
            if (planeText == null)
                return;

            InsertText(planeText);
        }

        #endregion
    }
}