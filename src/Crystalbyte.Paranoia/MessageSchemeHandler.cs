using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CefSharp;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using NLog;

namespace Crystalbyte.Paranoia {
    internal sealed class MessageSchemeHandler : ISchemeHandler {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Implementation of ISchemeHandler

        public bool ProcessRequestAsync(IRequest request, ISchemeHandlerResponse response,
            OnRequestCompletedHandler requestCompletedCallback) {
            try {
                var uri = new Uri(request.Url);

                if (Regex.IsMatch(uri.AbsolutePath, "[0-9]+")) {
                    Task.Run(() => {
                        try {
                            ComposeInspectionResponse(request, response);
                            requestCompletedCallback();
                        } catch (Exception ex) {
                            Logger.Error(ex);
                        }
                    });

                    return true;
                }

                if (Regex.IsMatch(uri.AbsolutePath, "new")) {
                    Task.Run(() => {
                        try {
                            Logger.Debug("Begin new message.");
                            ComposeBlankCompositionResponse(response);
                            requestCompletedCallback();
                            Logger.Debug("End new message.");
                        } catch (Exception ex) {
                            Logger.Error(ex);
                        }
                    });

                    return true;
                }

                if (Regex.IsMatch(uri.AbsolutePath, "reply")) {
                    Task.Run(() => {
                        try {
                            ComposeQuotedCompositionResponse(request, response);
                            requestCompletedCallback();
                        } catch (Exception ex) {
                            Logger.Error(ex);
                        }
                    });

                    return true;
                }

                if (Regex.IsMatch(uri.AbsolutePath, "forward")) {
                    Task.Run(() => {
                        try {
                            ComposeQuotedCompositionResponse(request, response);
                            requestCompletedCallback();
                        } catch (Exception ex) {
                            Logger.Error(ex);
                        }
                    });

                    return true;
                }

                if (Regex.IsMatch(uri.AbsolutePath, "part")) {
                    Task.Run(() => {
                        try {
                            ComposeCidImageResponse(request, response);
                            requestCompletedCallback();
                        } catch (Exception ex) {
                            Logger.Error(ex);
                        }
                    });

                    return true;
                }

                return false;
            } catch (Exception ex) {
                Logger.Error(ex);
                return false;
            }
        }

        #endregion

        private static void ComposeCidImageResponse(IRequest request, ISchemeHandlerResponse response) {
            var arguments = request.Url.ToPageArguments();

            long messageId;
            if (!arguments.ContainsKey("messageId") || !long.TryParse(arguments["messageId"], out messageId))
                return;

            if (!arguments.ContainsKey("cid"))
                return;

            var attachment = GetAttachmentBytes(arguments["cid"], messageId);
            if (attachment != null) {
                ComposeResourceResponse(response, attachment, "image");
            }
        }

        private static void ComposeResourceResponse(ISchemeHandlerResponse response, byte[] bytes, string mimeType) {
            response.MimeType = mimeType;
            response.ContentLength = bytes.Length;
            response.ResponseStream = new MemoryStream(bytes);
        }

        private static byte[] GetAttachmentBytes(string cid, long messageId) {
            using (var database = new DatabaseContext()) {
                var message = database.MimeMessages.FirstOrDefault(x => x.MessageId == messageId);

                if (message == null)
                    return null;

                var reader = new MailMessageReader(message.Data);
                var attachments = reader.FindAllAttachments();
                var attachment = attachments.FirstOrDefault(x => x.ContentId == Uri.UnescapeDataString(cid));
                if (attachment != null)
                    return attachment.Body;
            }

            return null;
        }

        private static void ComposeQuotedCompositionResponse(IRequest request, ISchemeHandlerResponse response) {
            var variables = new Dictionary<string, string> {
                {"header", string.Empty},
            };

            long messageId;
            var uri = new Uri(request.Url);
            var arguments = uri.OriginalString.ToPageArguments();
            if (arguments.ContainsKey("id") && long.TryParse(arguments["id"], out messageId)) {
                var message = GetMessageBytes(messageId);
                var reader = new MailMessageReader(message);

                var text = HtmlSupport.FindBestSupportedBody(reader);
                text = HtmlSupport.SuppressGlobalStyles(text);
                text = HtmlSupport.PrepareHtmlForInspection(text);
                text = HtmlSupport.ModifyEmbeddedParts(text, messageId);
                text = HtmlSupport.InsertQuoteSeparator(text);
                variables.Add("quote", text);
            }

            if (!variables.Keys.Contains("quote"))
                variables.Add("quote", string.Empty);

            var html = HtmlSupport.GetEditorTemplate(variables);
            var bytes = Encoding.UTF8.GetBytes(html);
            ComposeHtmlResponse(response, bytes);
        }

        private static string RemoveExternalSources(string content) {
            const string pattern = "src=(\"|')(?<URL>http(s){0,1}://.+?)(\"|')";
            content = Regex.Replace(content, pattern, m => {
                var resource = m.Groups["URL"].Value;
                return m.Value.Replace(resource, "");
            },
                RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return content;
        }

        private static void ComposeInspectionResponse(IRequest request, ISchemeHandlerResponse response) {
            var uri = new Uri(request.Url);
            var id = long.Parse(uri.Segments[1]);

            var arguments = uri.PathAndQuery.ToPageArguments();

            var mime = GetMessageBytes(id);
            var reader = new MailMessageReader(mime);
            var text = HtmlSupport.FindBestSupportedBody(reader);

            if (string.IsNullOrWhiteSpace(text)) {
                ComposeHtmlResponse(response, null);
                return;
            }

            const string key = "blockExternals";
            var blockExternals = !arguments.ContainsKey(key) || bool.Parse(arguments[key]);

            text = HtmlSupport.PrepareHtmlForInspection(text);
            text = HtmlSupport.ModifyEmbeddedParts(text, id);

            if (blockExternals) {
                text = RemoveExternalSources(text);
            }

            var bytes = Encoding.UTF8.GetBytes(text);
            ComposeHtmlResponse(response, bytes);
        }

        private static void ComposeBlankCompositionResponse(ISchemeHandlerResponse response) {
            var variables = new Dictionary<string, string> {
                {"quote", string.Empty},
            };

            var html = HtmlSupport.GetEditorTemplate(variables);
            var bytes = Encoding.UTF8.GetBytes(html);
            ComposeHtmlResponse(response, bytes);
        }

        private static byte[] GetMessageBytes(Int64 id) {
            using (var database = new DatabaseContext()) {
                var messages = database.MimeMessages
                    .Where(x => x.MessageId == id)
                    .ToArray();

                return messages.Length > 0 ? messages[0].Data : new byte[0];
            }
        }

        private static void ComposeHtmlResponse(ISchemeHandlerResponse response, byte[] bytes) {
            response.MimeType = "text/html";
            response.ContentLength = bytes.Length;
            response.ResponseStream = new MemoryStream(bytes);
        }
    }
}
