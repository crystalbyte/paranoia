using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using Crystalbyte.Paranoia.Mail;
using NLog;

namespace Crystalbyte.Paranoia {
    public sealed class EmlSchemeHandler : ISchemeHandler {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Implementation of ISchemeHandler

        public bool ProcessRequestAsync(IRequest request, ISchemeHandlerResponse response,
            OnRequestCompletedHandler requestCompletedCallback) {
            try {
                Task.Run(() => {
                    ComposeFileCompositionResponse(request, response);
                    requestCompletedCallback();
                });
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            return false;
        }

        #endregion

        #region Methods

        private static byte[] GetFileBytes(FileSystemInfo file) {
            return File.ReadAllBytes(file.FullName);
        }

        private static void ComposeFileCompositionResponse(IRequest request, ISchemeHandlerResponse response) {
            const string key = "path";
            var uri = new Uri(request.Url);
            var arguments = uri.PathAndQuery.ToPageArguments();
            if (!arguments.ContainsKey(key)) {
                throw new KeyNotFoundException(key);
            }

            var path = arguments[key];
            var filename = Uri.UnescapeDataString(path);

            var bytes = GetFileBytes(new FileInfo(filename));
            var reader = new MailMessageReader(bytes);

            var text = HtmlSupport.FindBestSupportedBody(reader);
            text = HtmlSupport.PrepareHtmlForInspection(text);
            text = HtmlSupport.ModifyEmbeddedParts(text, new FileInfo(path));

            var b = Encoding.UTF8.GetBytes(text);
            response.MimeType = "text/html";
            response.ContentLength = bytes.Length;
            response.ResponseStream = new MemoryStream(b);
        }

        #endregion
    }
}
