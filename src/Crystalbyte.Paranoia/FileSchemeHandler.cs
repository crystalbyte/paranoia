using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using NLog;

namespace Crystalbyte.Paranoia {
    public sealed class FileSchemeHandler : ISchemeHandler {
        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Implementation of ISchemeHandler

        public bool ProcessRequestAsync(IRequest request, ISchemeHandlerResponse response,
            OnRequestCompletedHandler requestCompletedCallback) {

            Task.Run(() => {
                try {
                    ComposeByteResponse(request, response);
                    requestCompletedCallback();
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            });

            return false;
        }

        private static void ComposeByteResponse(IRequest request, ISchemeHandlerResponse response) {
            var path = request.Url.Replace("file:///", string.Empty);

            var info = new FileInfo(path);
            if (!info.Exists) {
                response.StatusCode = 404;
                return;
            }

            if (info.Extension.EqualsIgnoreCase(".png")) {
                response.MimeType = "image/png";
                goto next;
            }

            if (info.Extension.EqualsIgnoreCase(".jpg")) {
                response.MimeType = "image/jpg";
                goto next;
            }

            if (info.Extension.EqualsIgnoreCase(".jpeg")) {
                response.MimeType = "image/jpg";
                goto next;
            }

            if (info.Extension.EqualsIgnoreCase(".html")) {
                response.MimeType = "text/html";
                goto next;
            }

            if (info.Extension.EqualsIgnoreCase(".txt")) {
                response.MimeType = "text/plain";
                goto next;
            }

            response.MimeType = "application/octet-stream";

            next:

            var bytes = File.ReadAllBytes(WebUtility.UrlDecode(path));

            response.ContentLength = bytes.Length;
            response.ResponseStream = new MemoryStream(bytes);
            response.StatusCode = 200;
        }

        #endregion

    }
}
