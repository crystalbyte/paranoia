﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
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
                        ComposeMessageResponse(request, response);
                        requestCompletedCallback();
                    });

                    return true;
                }

                if (Regex.IsMatch(uri.AbsolutePath, "new")) {
                    Task.Run(() => {
                        // TODO: Responding too quickly seems to break the editor on some machines.
                        Thread.Sleep(50);
                        ComposeBlankCompositionResponse(response);
                        requestCompletedCallback();
                    });

                    return true;
                }

                if (Regex.IsMatch(uri.AbsolutePath, "reply")) {
                    Task.Run(() => {
                        ComposeQuotedCompositionResponse(request, response);
                        requestCompletedCallback();
                    });

                    return true;
                }

                if (Regex.IsMatch(uri.AbsolutePath, "forward")) {
                    Task.Run(() => {
                        ComposeQuotedCompositionResponse(request, response);
                        requestCompletedCallback();
                    });

                    return true;
                }

                if (Regex.IsMatch(uri.AbsolutePath, "part")) {
                    Task.Run(() => {
                        ComposeCidImageResponse(request, response);
                        requestCompletedCallback();
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

            var html = GenerateEditorHtml(variables);
            var bytes = Encoding.UTF8.GetBytes(html);
            ComposeHtmlResponse(response, bytes);
        }

        private static string RemoveExternalSources(string content) {
            const string pattern = "(\"|&quot;|')(?<URL>http(s){0,1}://.+?)(\"|&quot;|')";
            content = Regex.Replace(content, pattern, m => {
                var resource = m.Groups["URL"].Value;
                return m.Value.Replace(resource, "");
            },
                RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return content;
        }

        private static void ComposeMessageResponse(IRequest request, ISchemeHandlerResponse response) {
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
                {"header", string.Empty},
            };

            var html = GenerateEditorHtml(variables);
            var bytes = Encoding.UTF8.GetBytes(html);
            ComposeHtmlResponse(response, bytes);
        }

        private static string GenerateEditorHtml(IDictionary<string, string> variables) {
            var uri = new Uri("/Resources/composition.template.html", UriKind.Relative);
            var info = Application.GetResourceStream(uri);
            if (info == null) {
                var error = string.Format(Resources.ResourceNotFoundException, uri, typeof(App).Assembly.FullName);
                throw new ResourceNotFoundException(error);
            }

            string html;
            const string pattern = "{{.+?}}";
            using (var reader = new StreamReader(info.Stream)) {
                var text = reader.ReadToEnd();
                html = Regex.Replace(text, pattern, m => {
                    var key = m.Value.Trim('{', '}').ToLower();
                    return variables.ContainsKey(key)
                        ? variables[key]
                        : string.Empty;
                }, RegexOptions.IgnoreCase);
            }

            return html;
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
