#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
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
using System.Diagnostics;
using CefSharp;
using Crystalbyte.Paranoia.Properties;
using NLog;

#endregion

namespace Crystalbyte.Paranoia.UI {
    internal sealed class HtmlRequestHandler : IRequestHandler {
        #region Private Fields

        private readonly IRequestAware _viewer;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Construction

        public HtmlRequestHandler(IRequestAware viewer) {
            _viewer = viewer;
        }

        #endregion

        #region Implementation of IRequestHandler

        public bool OnBeforeBrowse(IWebBrowser browser, IRequest request, bool isRedirect, bool isMainFrame) {
            try {
                Logger.Debug(string.Format(Resources.NavigationInfoTemplate, request.Url));

                var uri = new Uri(request.Url);
                var isExternal = string.Compare(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase) == 0
                                 || string.Compare(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase) == 0;

                if (isExternal) {
                    Process.Start(uri.AbsoluteUri);
                    return true;
                }
            }
            catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }

            return false;
        }

        public bool OnCertificateError(IWebBrowser browser, CefErrorCode errorCode, string requestUrl) {
            try {
                var uri = new Uri(requestUrl);
                var message = string.Format(Resources.CertificateErrorTemplate, uri.Host);
                Logger.Error(message);
            }
            catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
            }

            return false;
        }

        public void OnPluginCrashed(IWebBrowser browser, string pluginPath) {
            Logger.Error(pluginPath);
        }

        public bool OnBeforeResourceLoad(IWebBrowser browser, IRequest request, bool isMainFrame) {
            try {
                Logger.Debug(string.Format(Resources.ResourceRequestTemplate, request.Url));
                return false;
            }
            catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
                return false;
            }
        }

        public bool GetAuthCredentials(IWebBrowser browser, bool isProxy, string host, int port, string realm,
            string scheme,
            ref string username, ref string password) {
            return false;
        }

        public bool OnBeforePluginLoad(IWebBrowser browser, string url, string policyUrl, WebPluginInfo info) {
            return true;
        }

        public void OnRenderProcessTerminated(IWebBrowser browser, CefTerminationStatus status) {
            Logger.Debug(Resources.RenderProcessTerminatedTemplate, status);
        }

        #endregion
    }
}