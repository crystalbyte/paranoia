#region Using directives

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Awesomium.Core;
using Awesomium.Windows.Controls;
using System.Text.RegularExpressions;
using System.IO;

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
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            var url = WebCore.Configuration.HomeURL;
            if (url == null) {
                return;
            }

            Source = WebCore.Configuration.HomeURL.ToString();
        }

        #endregion

        #region Destruction

        ~HtmlControl() {
            Dispose(false);
        }

        #endregion

        #region Events

        public event EventHandler<PrintCompleteEventArgs> PrintSourcesCreated;

        protected virtual void OnPrintSourcesCreated(PrintCompleteEventArgs e) {
            var handler = PrintSourcesCreated;
            if (handler != null) handler(this, e);
        }

        public event EventHandler DocumentReady;

        protected virtual void OnDocumentReady() {
            var handler = DocumentReady;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler EditorContentChanged;

        protected virtual void OnEditorContentChanged() {
            var handler = EditorContentChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public event EventHandler<ScriptingFailureEventArgs> ScriptingFailure;

        protected virtual void OnScriptingFailure(ScriptingFailureEventArgs e) {
            var handler = ScriptingFailure;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        #region Properties

        public bool IsDocumentReady {
            get {
                return _webControl != null && _webControl.IsDocumentReady;
            }
        }

        private bool CanExecuteCommands {
            get {
                if (_webControl == null) {
                    return false;
                }

                if (!_webControl.IsJavascriptEnabled || !_webControl.IsDocumentReady) {
                    return false;
                }

                JSObject module = _webControl.ExecuteJavascriptWithResult("Crystalbyte.Paranoia");
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // ReSharper disable once HeuristicUnreachableCode
                if (module == null) {
                    // ReSharper disable HeuristicUnreachableCode
                    return false;
                    // ReSharper restore HeuristicUnreachableCode
                }

                module.Dispose();
                return true;
            }
        }

        #endregion

        #region Dependency Properties

        public bool AdjustToDpi {
            get { return (bool)GetValue(AdjustToDpiProperty); }
            set { SetValue(AdjustToDpiProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AdjustToDpi.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AdjustToDpiProperty =
            DependencyProperty.Register("AdjustToDpi", typeof(bool), typeof(HtmlControl), new PropertyMetadata(false));

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
            PasteFromClipboard();
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            if (_webControl != null) {
                _webControl.GotKeyboardFocus -= OnGotKeyboardFocus;
                _webControl.ShowCreatedWebView -= OnWebControlShowCreatedWebView;
                _webControl.DocumentReady -= OnWebControlDocumentReady;
                _webControl.WindowClose -= OnWebControlWindowClose;
            }

            _webControl = (WebControl)Template.FindName(WebControlPartName, this);
            _webControl.DocumentReady += OnWebControlDocumentReady;
            _webControl.GotKeyboardFocus += OnGotKeyboardFocus;
            _webControl.ShowCreatedWebView += OnWebControlShowCreatedWebView;
            _webControl.WindowClose += OnWebControlWindowClose;
        }

        private void OnWebControlDocumentReady(object sender, UrlEventArgs e) {
            var webControl = sender as WebControl;

            if (webControl != null) {
                SetScriptingObject(webControl);
            }

            OnDocumentReady();

            CommandManager.InvalidateRequerySuggested();
        }

        #endregion

        #region Methods

        private void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (!_webControl.IsDocumentReady)
                return;

            if (CanExecuteCommands) {
                FocusEditor();
            }
        }

        private static void SetScriptingObject(IWebView webcontrol) {
            using (JSObject interop = webcontrol.CreateGlobalJavascriptObject("external")) {
                interop.Bind("OnLinkClicked", false, OnLinkClicked);
            }
        }

        private static void OnLinkClicked(object sender, JavascriptMethodEventArgs e) {
            var href = (string)e.Arguments[0];
            if (string.IsNullOrEmpty(href))
                return;

            Process.Start(href);
        }

        public void ExecuteEditCommand(params object[] arguments) {
            JSObject module = _webControl.ExecuteJavascriptWithResult("Crystalbyte.Paranoia");
            using (module) {
                var function = string.Format("execute");
                var parameters = arguments.ToJsValues().ToArray();
                module.Invoke(function, parameters);
            }
        }

        private void FocusEditor() {
            JSObject module = _webControl.ExecuteJavascriptWithResult("Crystalbyte.Paranoia");
            using (module) {
                const string focus = "focusEditor";
                module.Invoke(focus);
            }
        }

        private static void OnWebControlWindowClose(object sender, WindowCloseEventArgs e) {
            // Nada ...
        }

        private static void OnWebControlShowCreatedWebView(object sender, ShowCreatedWebViewEventArgs e) {
            // Nada ...
        }

        public string GetDocument() {
            return _webControl.HTML;
        }

        public string GetComposition() {
            JSObject module = _webControl.ExecuteJavascriptWithResult("Crystalbyte.Paranoia");
            using (module) {
                const string function = "getComposition";
                return module.Invoke(function);
            }
        }

        public void ChangeSignature(string signature) {
            JSObject module = _webControl.ExecuteJavascriptWithResult("Crystalbyte.Paranoia");
            using (module) {
                const string function = "setSignature";
                module.Invoke(function, new[] { new JSValue(signature) });
            }
        }

        public void InsertHtml(string html) {
            JSObject module = _webControl.ExecuteJavascriptWithResult("Crystalbyte.Paranoia");
            using (module) {
                const string function = "insertHtml";
                module.Invoke(function, html);
            }
        }

        public void InsertText(string text) {
            JSObject module = _webControl.ExecuteJavascriptWithResult("Crystalbyte.Paranoia");
            using (module) {
                const string function = "insertText";
                module.Invoke(function, text);
            }
        }

        private void PasteFromClipboard() {
            var data = Clipboard.GetDataObject();
            if (data == null)
                return;

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
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
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
                if (!string.IsNullOrEmpty(html)) {
                    InsertHtml(html);
                    return;
                }
            }

            var plainText = (string)data.GetData(DataFormats.Text);
            if (plainText == null)
                return;

            InsertText(plainText);
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
                // BUG: Dispose called on the wrong thread.
                if (!_webControl.CheckAccess()) {
                    return;
                }
                _webControl.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}