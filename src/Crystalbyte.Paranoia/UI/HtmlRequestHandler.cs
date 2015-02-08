using System;
using System.Diagnostics;
using CefSharp;
using Crystalbyte.Paranoia.Properties;
using NLog;

namespace Crystalbyte.Paranoia.UI {
    internal sealed class HtmlRequestHandler : IRequestHandler {

        #region Private Fields

        private readonly HtmlViewer _viewer;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public HtmlRequestHandler(HtmlViewer viewer) {
            _viewer = viewer;
        }

        #endregion

        #region Implementation of IRequestHandler

        public bool OnBeforeBrowse(IWebBrowser browser, IRequest request, bool isRedirect) {
            try {
                Logger.Info(string.Format(Resources.NavigationInfoTemplate, request.Url));

                var uri = new Uri(request.Url);
                var isExternal = string.Compare(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase) == 0
                               || string.Compare(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase) == 0;

                if (isExternal) {
                    Process.Start(uri.AbsoluteUri);
                    return true;
                }

                var isAsset = string.Compare(uri.Scheme, "asset", StringComparison.OrdinalIgnoreCase) == 0;
                if (!isAsset) {
                    Logger.Info(Resources.NotSupportedSchemeTemplate, uri.AbsoluteUri);
                    return false;
                }
            }
            catch (Exception ex) {
                Logger.Error(ex);
            }

            return false;
        }

        public bool OnCertificateError(IWebBrowser browser, CefErrorCode errorCode, string requestUrl) {
            try {
                var uri = new Uri(requestUrl);
                var message = string.Format(Resources.CertificateErrorTemplate, uri.Host);
                Logger.Error(message);
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            return false;
        }

        public void OnPluginCrashed(IWebBrowser browser, string pluginPath) {
            Logger.Error(pluginPath);
        }

        public bool OnBeforeResourceLoad(IWebBrowser browser, IRequest request, IResponse response) {
            try {
                Logger.Info(string.Format(Resources.ResourceRequestTemplate, request.Url));
                return false;
            } catch (Exception ex) {
                Logger.Error(ex);
                return false;
            }
        }

        public bool GetAuthCredentials(IWebBrowser browser, bool isProxy, string host, int port, string realm, string scheme,
            ref string username, ref string password) {
            return false;
        }

        public bool OnBeforePluginLoad(IWebBrowser browser, string url, string policyUrl, IWebPluginInfo info) {
            return false;
        }

        public void OnRenderProcessTerminated(IWebBrowser browser, CefTerminationStatus status) {
            Logger.Info(Resources.RenderProcessTerminatedTemplate, status);
        }

        #endregion
    }
}
