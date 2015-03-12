using System;
using System.IO;
using System.Threading.Tasks;
using CefSharp;
using Crystalbyte.Paranoia.Data;
using NLog;

namespace Crystalbyte.Paranoia {
    public sealed class CompositionSchemeHandler : ISchemeHandler {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        public bool ProcessRequestAsync(IRequest request, ISchemeHandlerResponse response,
            OnRequestCompletedHandler requestCompletedCallback) {
            try {
                Task.Run(() => {
                    ComposeCompositionResponse(request, response);
                    requestCompletedCallback();
                });

                return true;
            } catch (Exception ex) {
                Logger.Error(ex);
                return false;
            }
        }

        private static void ComposeCompositionResponse(IRequest request, ISchemeHandlerResponse response) {
            var uri = new Uri(request.Url);
            var id = long.Parse(uri.LocalPath.Split('/')[1]);

            using (var database = new DatabaseContext()) {
                var smtp = database.Compositions.Find(id);
                if (smtp == null) {
                    throw new MessageNotFoundException(id);
                }
                
                response.MimeType = "text/html";
                response.ContentLength = smtp.Mime.Length;
                response.ResponseStream = new MemoryStream(smtp.Mime);
            }
        }
    }
}
