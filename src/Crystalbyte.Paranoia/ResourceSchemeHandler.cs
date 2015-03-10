using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Resources;
using CefSharp;
using Crystalbyte.Paranoia.UI;
using dotless.Core;
using NLog;

namespace Crystalbyte.Paranoia {
    public sealed class ResourceSchemeHandler : ISchemeHandler {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly object Mutex = new object();

        #endregion

        #region Implementation of ISchemeHandler

        public bool ProcessRequestAsync(IRequest request, ISchemeHandlerResponse response,
            OnRequestCompletedHandler requestCompletedCallback) {

            Task.Run(() => {
                try {
                    ComposeResourceResponse(request, response);
                    requestCompletedCallback();
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            });

            return true;

        }

        private static void ComposeResourceResponse(IRequest request, ISchemeHandlerResponse response) {
            var uri = new Uri(request.Url);
            var name = uri.Segments[1].Trim('/');
            var resource = new Uri(string.Format("/Resources/{0}", name), UriKind.RelativeOrAbsolute);

            // BUG: Occasionally throws ExecutionEngineException if not locked, so sad ... :(
            StreamResourceInfo info;
            lock (Mutex) {
                info = Application.GetResourceStream(resource);
                if (info == null) {
                    throw new ResourceNotFoundException(resource.AbsoluteUri);
                }
            }

            response.CloseStream = true;
            if (name.EndsWith("less")) {
                using (var reader = new StreamReader(info.Stream)) {
                    var less = reader.ReadToEnd();

                    less = Regex.Replace(less, "::[A-Za-z0-9]+::", m => {
                        var key = m.Value.Trim(':');
                        var brush = Application.Current.Resources[key] as SolidColorBrush;
                        return brush != null ? brush.Color.ToHex(false) : "Fuchsia";
                    });

                    var css = Less.Parse(less);

                    var bytes = Encoding.UTF8.GetBytes(css);
                    response.ResponseStream = new MemoryStream(bytes);
                    response.MimeType = "text/css";
                }
                return;
            }

            response.ResponseStream = info.Stream;
        }

        #endregion
    }
}
