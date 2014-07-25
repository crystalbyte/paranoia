using System;
using System.ComponentModel;
using Awesomium.Core;
using Awesomium.Windows.Controls;
using System.Windows;
using System.Windows.Controls;

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
            if (!DesignerProperties.GetIsInDesignMode(this)) {
                Source = WebCore.Configuration.HomeURL.ToString();    
            }
        }

        #endregion

        #region Destruction
        ~HtmlControl() {
            Dispose(false);
        }

        #endregion

        #region Dependency Properties

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

        #endregion

        #region Class Overrides

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            if (_webControl != null) {
                _webControl.ShowCreatedWebView -= OnWebControlShowCreatedWebView;
                _webControl.WindowClose -= OnWebControlWindowClose;
            }

            _webControl = (WebControl) Template.FindName(WebControlPartName, this);
            _webControl.ShowCreatedWebView += OnWebControlShowCreatedWebView;
            _webControl.WindowClose += OnWebControlWindowClose;
        }

        private void OnWebControlWindowClose(object sender, WindowCloseEventArgs e) {
            
        }

        private void OnWebControlShowCreatedWebView(object sender, ShowCreatedWebViewEventArgs e) {
            
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
    }
}
