﻿#region Copyright Notice & Copying Permission

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
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    internal sealed class MessageSchemeHandler : ISchemeHandler {
        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Implementation of ISchemeHandler

        public bool ProcessRequestAsync(IRequest request, ISchemeHandlerResponse response,
            OnRequestCompletedHandler requestCompletedCallback) {
            try {
                Logger.Debug(string.Format("Invoked ProcessRequestAsync on thread {0}.",
                    Thread.CurrentThread.ManagedThreadId));

                var uri = new Uri(request.Url);

                if (Regex.IsMatch(uri.AbsolutePath, "[0-9]+")) {
                    Task.Run(async () => {
                        try {
                            await CreateMessageResponseAsync(request, response);
                            requestCompletedCallback();
                        } catch (Exception ex) {
                            Logger.ErrorException(ex.Message, ex);
                        }
                    });

                    return true;
                }

                if (Regex.IsMatch(uri.AbsolutePath, "new")) {
                    Task.Run(() => {
                        try {
                            ComposeBlankCompositionResponse(response);
                            requestCompletedCallback();
                        } catch (Exception ex) {
                            Logger.ErrorException(ex.Message, ex);
                        }
                    });

                    return true;
                }

                if (Regex.IsMatch(uri.AbsolutePath, "reply")) {
                    Task.Run(async () => {
                        try {
                            await ComposeQuotedCompositionResponseAsync(request, response);
                            requestCompletedCallback();
                        } catch (Exception ex) {
                            Logger.ErrorException(ex.Message, ex);
                        }
                    });

                    return true;
                }

                if (Regex.IsMatch(uri.AbsolutePath, "forward")) {
                    Task.Run(async () => {
                        try {
                            await ComposeQuotedCompositionResponseAsync(request, response);
                            requestCompletedCallback();
                        } catch (Exception ex) {
                            Logger.ErrorException(ex.Message, ex);
                        }
                    });

                    return true;
                }

                if (Regex.IsMatch(uri.AbsolutePath, "part")) {
                    Task.Run(async () => {
                        try {
                            await ComposeCidImageResponseAsync(request, response);
                            requestCompletedCallback();
                        } catch (Exception ex) {
                            Logger.ErrorException(ex.Message, ex);
                        }
                    });

                    return true;
                }

                return false;
            } catch (Exception ex) {
                Logger.ErrorException(ex.Message, ex);
                return false;
            }
        }

        #endregion

        private static async Task ComposeCidImageResponseAsync(IRequest request, ISchemeHandlerResponse response) {
            var arguments = request.Url.ToPageArguments();

            long messageId;
            if (!arguments.ContainsKey("messageId") || !long.TryParse(arguments["messageId"], out messageId))
                return;

            if (!arguments.ContainsKey("cid"))
                return;

            var attachment = await GetAttachmentBytesAsync(arguments["cid"], messageId);
            if (attachment != null) {
                ComposeResourceResponse(response, attachment, "image");
            }
        }

        private static void ComposeResourceResponse(ISchemeHandlerResponse response, byte[] bytes, string mimeType) {
            response.MimeType = mimeType;
            response.ContentLength = bytes.Length;
            response.ResponseStream = new MemoryStream(bytes);
        }

        private static async Task<byte[]> GetAttachmentBytesAsync(string cid, Int64 messageId) {
            using (var database = new DatabaseContext()) {
                var mime = await database.MailMessages
                    .Where(x => x.Id == messageId)
                    .Select(x => x.Mime)
                    .FirstOrDefaultAsync();

                if (mime == null)
                    return null;

                var reader = new MailMessageReader(mime);
                var attachments = reader.FindAllAttachments();
                var attachment = attachments.FirstOrDefault(x => x.ContentId == Uri.UnescapeDataString(cid));
                if (attachment != null)
                    return attachment.Body;
            }

            return null;
        }

        private async static Task ComposeQuotedCompositionResponseAsync(IRequest request, ISchemeHandlerResponse response) {
            var variables = new Dictionary<string, string>();

            long messageId;
            var uri = new Uri(request.Url);
            var arguments = uri.OriginalString.ToPageArguments();
            if (arguments.ContainsKey("id") && long.TryParse(arguments["id"], out messageId)) {
                var message = await GetMessageBytesAsync(messageId);
                var reader = new MailMessageReader(message);

                var header = string.Format(Resources.HtmlResponseHeader, reader.Headers.DateSent,
                    reader.Headers.From.DisplayName);

                var text = HtmlSupport.FindBestSupportedBody(reader);
                text = HtmlSupport.ModifyEmbeddedParts(text, messageId);
                variables.Add("quote", text);
            }

            if (!variables.Keys.Contains("quote"))
                variables.Add("quote", string.Empty);

            var html = HtmlSupport.GetEditorTemplate(variables);
            var bytes = Encoding.UTF8.GetBytes(html);
            CreateHtmlResponse(response, bytes);
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

        private async static Task CreateMessageResponseAsync(IRequest request, ISchemeHandlerResponse response) {
            var uri = new Uri(request.Url);
            var id = long.Parse(uri.Segments[1]);

            var arguments = uri.PathAndQuery.ToPageArguments();

            var mime = await GetMessageBytesAsync(id);
            var reader = new MailMessageReader(mime);
            var text = HtmlSupport.FindBestSupportedBody(reader);

            if (string.IsNullOrWhiteSpace(text)) {
                CreateHtmlResponse(response, null);
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
            CreateHtmlResponse(response, bytes);
        }

        private static void ComposeBlankCompositionResponse(ISchemeHandlerResponse response) {
            var variables = new Dictionary<string, string> {
                {"quote", string.Empty},
            };

            var html = HtmlSupport.GetEditorTemplate(variables);
            var bytes = Encoding.UTF8.GetBytes(html);
            CreateHtmlResponse(response, bytes);
        }

        private static async Task<byte[]> GetMessageBytesAsync(Int64 id) {
            using (var database = new DatabaseContext()) {
                var mime = await database.MailMessages
                    .Where(x => x.Id == id)
                    .Select(x => x.Mime)
                    .FirstOrDefaultAsync();

                return mime ?? new byte[0];
            }
        }

        private static void CreateHtmlResponse(ISchemeHandlerResponse response, byte[] bytes) {
            response.MimeType = "text/html";
            response.ContentLength = bytes.Length;
            response.ResponseStream = new MemoryStream(bytes);
        }
    }
}