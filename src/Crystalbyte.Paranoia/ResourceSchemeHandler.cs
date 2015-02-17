using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Resources;
using CefSharp;
using dotless.Core;
using ICSharpCode.SharpZipLib.Zip;
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
            try {
                Task.Run(() => {
                    ComposeResourceResponse(request, response);
                    requestCompletedCallback();
                });

                return true;
            } catch (Exception ex) {
                Logger.Error(ex);
                return false;
            }
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
            if (name.EndsWith("zip")) {
                var path = uri.Segments[2].Trim();
                if (path.EndsWith("css")) {
                    response.MimeType = "text/css";
                }
                if (path.EndsWith("js")) {
                    response.MimeType = "text/javascript";
                }
                using (var zip = new ZipFile(info.Stream)) {
                    zip.UseZip64 = UseZip64.Off;
                    var entry = zip.GetEntry(path);
                    var s = zip.GetInputStream(entry);

                    byte[] bytes;
                    using (var reader = new BinaryReader(s)) {
                        // NOTE: Will break for files larger than 4GB :P
                        bytes = reader.ReadBytes((int)info.Stream.Length);
                    }

                    var text = Encoding.UTF8.GetString(bytes);
                    response.ResponseStream = new MemoryStream(bytes);
                }
                return;
            }

            if (name.EndsWith("less")) {
                using (var reader = new StreamReader(info.Stream)) {
                    var less = reader.ReadToEnd();
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
