#region Using directives

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Awesomium.Core;
using Awesomium.Windows.Controls;
using ContextMenuEventArgs = Awesomium.Core.ContextMenuEventArgs;

#endregion

namespace Crystalbyte.Paranoia.UI {
    [TemplatePart(Name = WebControlPartName, Type = typeof(WebControl))]
    [TemplatePart(Name = ContextMenuPartName, Type = typeof(ContextMenu))]
    public class HtmlViewer : Control {

        #region Xaml Support

        private const string WebControlPartName = "PART_WebControl";
        private const string ContextMenuPartName = "PART_ContextMenu";

        #endregion

        #region Private Fields

        private bool _disposed;
        private WebControl _webControl;
        private ContextMenu _contextMenu;

        #endregion

        #region Construction

        static HtmlViewer() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HtmlViewer),
                new FrameworkPropertyMetadata(typeof(HtmlViewer)));
        }

        public HtmlViewer() {
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

        ~HtmlViewer() {
            Dispose(false);
        }

        #endregion

        #region Events

        public event EventHandler<PrintCompleteEventArgs> PrintSourcesCreated;

        protected virtual void OnPrintSourcesCreated(PrintCompleteEventArgs e) {
            var handler = PrintSourcesCreated;
            if (handler != null) handler(this, e);
        }
      
        public event EventHandler<ScriptingFailureEventArgs> ScriptingFailure;

        protected virtual void OnScriptingFailure(ScriptingFailureEventArgs e) {
            var handler = ScriptingFailure;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        #region Dependency Properties

        public bool AdjustToDpi {
            get { return (bool)GetValue(AdjustToDpiProperty); }
            set { SetValue(AdjustToDpiProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AdjustToDpi.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AdjustToDpiProperty =
            DependencyProperty.Register("AdjustToDpi", typeof(bool), typeof(HtmlViewer), new PropertyMetadata(false));

        public WebSession WebSession {
            get { return (WebSession)GetValue(WebSessionProperty); }
            set { SetValue(WebSessionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WebSession.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WebSessionProperty =
            DependencyProperty.Register("WebSession", typeof(WebSession), typeof(HtmlViewer), new PropertyMetadata(null));

        public float Zoom {
            get { return (float)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Zoom.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(float), typeof(HtmlViewer), new PropertyMetadata(1.45f));

        public bool IsTransparent {
            get { return (bool)GetValue(IsTransparentProperty); }
            set { SetValue(IsTransparentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTransparent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTransparentProperty =
            DependencyProperty.Register("IsTransparent", typeof(bool), typeof(HtmlViewer),
                new PropertyMetadata(false));

        // This will be set to the target URL, when this window does not
        // host a created child view. The WebControl, is bound to this property.
        public string Source {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Identifies the <see cref="Source"/> dependency property.
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(HtmlViewer),
                new FrameworkPropertyMetadata(string.Empty));

        #endregion

        #region Class Overrides

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            _contextMenu = (ContextMenu) Template.FindName(ContextMenuPartName, this);

            if (_webControl != null) {
                _webControl.WindowClose -= OnWebControlWindowClose;
                _webControl.ConsoleMessage -= OnConsoleMessage;
                _webControl.ShowContextMenu -= OnShowContextMenu;
                _webControl.LoadingFrameComplete -= LoadingFrameComplete;
                _webControl.ShowCreatedWebView -= OnWebControlShowCreatedWebView;
            }

            _webControl = (WebControl)Template.FindName(WebControlPartName, this);
            _webControl.ShowCreatedWebView += OnWebControlShowCreatedWebView;
            _webControl.LoadingFrameComplete += LoadingFrameComplete;
            _webControl.ShowContextMenu += OnShowContextMenu;
            _webControl.ConsoleMessage += OnConsoleMessage;
            _webControl.WindowClose += OnWebControlWindowClose;
        }

        private void OnShowContextMenu(object sender, ContextMenuEventArgs e) {
            _contextMenu.IsOpen = true;
        }

        private void LoadingFrameComplete(object sender, FrameEventArgs e) {
            SetExternalScriptingObject();
        }

        private void SetExternalScriptingObject() {
            using (JSObject interop = _webControl.CreateGlobalJavascriptObject("external")) {
                interop.Bind("onLinkClicked", false, OnLinkClicked);
            }
        }

        private static void OnConsoleMessage(object sender, ConsoleMessageEventArgs e) {
            Debug.WriteLine("JavaScript:{0}:{1}:{2}", e.LineNumber, e.Source, e.Message);
        }

        #endregion

        #region Methods

        private static void OnLinkClicked(object sender, JavascriptMethodEventArgs e) {
            var href = (string)e.Arguments[0];
            if (string.IsNullOrEmpty(href))
                return;

            Process.Start(href);
        }

        private static void OnWebControlWindowClose(object sender, WindowCloseEventArgs e) {
            // Nada ...
        }

        private static void OnWebControlShowCreatedWebView(object sender, ShowCreatedWebViewEventArgs e) {
            e.Cancel = true;
        }

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