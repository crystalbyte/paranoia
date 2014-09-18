#region Using directives

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Awesomium.Core.Data;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using HtmlAgilityPack;
using NLog;

#endregion

namespace Crystalbyte.Paranoia {
    internal sealed class ParanoiaDataSource : DataSource {

        private readonly static Logger Logger = LogManager.GetCurrentClassLogger();

        protected override async void OnRequest(DataSourceRequest request) {
            try {
                if (Regex.IsMatch(request.Path, "message/[0-9]+")) {
                    var id = request.Path.Split('/')[1];
                    await SendMessageQueryResponseAsync(request, id);
                    return;
                }

                if (Regex.IsMatch(request.Path, "message/new")) {
                    SendComposeAsNewResponse(request);
                    return;
                }

                if (Regex.IsMatch(request.Path, "message/reply")) {
                    SendComposeAsReplyResponse(request);
                    return;
                }

                SendResponse(request, DataSourceResponse.Empty);
            } catch (Exception ex) {
                //TODO: Return 793 - Zombie Apocalypse
                Logger.Error(ex);
            }
        }

        private void SendComposeAsReplyResponse(DataSourceRequest request) {
            long messageId;
            var variables = new Dictionary<string, string>();
            var arguments = request.Url.OriginalString.ToPageArguments();
            if (arguments.ContainsKey("id") && long.TryParse(arguments["id"], out messageId)) {
                var bodyHtml = GetBodyHtmlFromId(messageId);
                bodyHtml = ConvertEmbeddedSources(bodyHtml, messageId.ToString(CultureInfo.InvariantCulture));
                //todo insert reply header
                variables.Add("content", bodyHtml);
            }

            if (!variables.Keys.Contains("content"))
                variables.Add("content", string.Empty);

            var html = GenerateEditorHtml(variables);
            //var escapedHtml = Uri.EscapeDataString(html);
            var bytes = Encoding.UTF8.GetBytes(html);
            SendByteStream(request, bytes);
        }

        private string GetBodyHtmlFromId(long id) {
            using (var database = new DatabaseContext()) {
                var message = database.MimeMessages.FirstOrDefault(x => x.MessageId == id);
                if (message != null) {
                    var reader = new MailMessageReader(message.Data);
                    var body = reader.FindFirstHtmlVersion().Body;
                    if (body != null) {
                        return Encoding.UTF8.GetString(body);
                    }
                }
                return "no html body found";
            }
        }

        private void SendComposeAsNewResponse(DataSourceRequest request) {
            var variables = new Dictionary<string, string> {
                {"content", string.Empty}
            };

            var html = GenerateEditorHtml(variables);
            //var escapeHtml = Uri.EscapeDataString(html);
            var bytes = Encoding.UTF8.GetBytes(html);
            SendByteStream(request, bytes);
        }

        private static string GenerateEditorHtml(IDictionary<string, string> variables) {
            var uri = new Uri("/Resources/composition.template.html", UriKind.Relative);
            var info = Application.GetResourceStream(uri);
            if (info == null) {
                var error = string.Format(Resources.ResourceNotFoundException, uri, typeof(App).Assembly.FullName);
                throw new Exception(error);
            }

            string html;
            const string pattern = "%.+?%";
            using (var reader = new StreamReader(info.Stream)) {
                var text = reader.ReadToEnd();
                html = Regex.Replace(text, pattern, m => {
                    var key = m.Value.Trim('%').ToLower();
                    return variables.ContainsKey(key)
                        ? variables[key]
                        : string.Empty;
                }, RegexOptions.IgnoreCase);
            }

            return html;
        }

        private async Task SendMessageQueryResponseAsync(DataSourceRequest request, string id) {
            var mime = await LoadMessageContentAsync(Int64.Parse(id));
            var reader = new MailMessageReader(mime);

            string content;
            Encoding encoding;
            var html = reader.FindFirstHtmlVersion();
            if (html == null) {
                var plain = reader.FindFirstPlainTextVersion();
                content = plain == null ? string.Empty : await FormatPlainText(reader.Headers.Subject, plain.GetBodyAsText());
                encoding = Encoding.UTF8;

            } else {
                content = html.GetBodyAsText();
                try {
                    encoding = Encoding.GetEncoding(html.ContentType.CharSet ?? "utf-8");
                } catch (Exception) {
                    encoding = Encoding.UTF8;
                }
            }

            if (string.IsNullOrWhiteSpace(content)) {
                SendResponse(request, DataSourceResponse.Empty);
                return;
            }

            content = NormalizeHtml(content, encoding);
            content = ConvertEmbeddedSources(content, id);
            var bytes = encoding.GetBytes(content);
            SendByteStream(request, bytes);
        }

        private static string NormalizeHtml(string text, Encoding encoding) {
            var document = new HtmlDocument { OptionFixNestedTags = true };
            document.LoadHtml(text.Trim());

            HtmlDocument partialDocument = null;
            var html = document.DocumentNode.SelectSingleNode("//html");
            if (html == null) {
                partialDocument = document;

                // If no html tag has been found, we start from scratch.
                document = new HtmlDocument();
                html = document.CreateElement("html");
                document.DocumentNode.AppendChild(html);
            }

            var body = document.DocumentNode.SelectSingleNode("//body");
            var head = document.DocumentNode.SelectSingleNode("//head");
            if (head == null) {
                if (partialDocument != null) {
                    head = partialDocument.DocumentNode.SelectSingleNode("//head");
                    if (head == null) {
                        head = document.CreateElement("head");
                    } else {
                        // Remove from partial doc, since the constructed document already has one.
                        head.Remove();
                    }
                } else {
                    head = document.CreateElement("head");
                }

                if (body != null) {
                    html.InsertBefore(head, body);
                } else {
                    html.AppendChild(head);
                }
            }

            if (body == null) {
                if (partialDocument != null) {
                    body = partialDocument.DocumentNode.SelectSingleNode("//body");
                    if (body == null) {
                        body = document.CreateElement("body");
                    } else {
                        // Remove from partial doc, since the constructed document already has one.
                        body.Remove();
                    }

                } else {
                    body = document.CreateElement("body");
                }
                html.AppendChild(body);
            }

            if (partialDocument != null) {
                body.AppendChildren(partialDocument.DocumentNode.ChildNodes);
            }

            const string charset = "charset";
            const string httpEquiv = "http-equiv";
            const string contentType = "content-type";

            var nodes = document.DocumentNode.SelectNodes("//head/meta");
            var hasCharset = nodes != null && nodes.Any(x => {
                // Check if any attributes are present.
                if (!x.HasAttributes) {
                    return false;
                }

                // Check if the <meta charset="..."> element is present.
                var attribute =
                    x.Attributes.AttributesWithName(charset)
                        .FirstOrDefault();
                if (attribute != null) {
                    return true;
                }

                // Check if the <meta http-equiv="content-type" ... > element is present.
                attribute =
                    x.Attributes.AttributesWithName(httpEquiv)
                        .FirstOrDefault();
                if (attribute != null &&
                    attribute.Value.ContainsIgnoreCase(contentType)) {

                    attribute = x.Attributes.AttributesWithName("content").FirstOrDefault();
                    return attribute != null;
                }
                return false;
            });

            if (hasCharset) {
                return document.DocumentNode.WriteTo();
            }

            // Add proper charset tag if missing.
            var meta = document.CreateElement("meta");

            // Since we use WebKit any Windows only encodings, 
            // such as ISO-8859-1, won't work and must be replaced by UTF8.
            var name = encoding.WebName;
            meta.Attributes.Add(charset, name);
            head.AppendChild(meta);
            return document.DocumentNode.WriteTo();
        }

        private async static Task<string> FormatPlainText(string subject, string plain) {
            const string url = "/Resources/plain-text.template.html";
            var info = Application.GetResourceStream(new Uri(url, UriKind.RelativeOrAbsolute));
            if (info == null) {
                var message = string.Format(Resources.ResourceNotFoundException, url, typeof(App).Assembly.FullName);
                throw new NullReferenceException(message);
            }

            var wrapper = await info.Stream.ToUtf8StringAsync();
            var values = new Dictionary<string, string> {
                {"content", plain},
                {"subject", subject}
            };

            return Regex.Replace(wrapper, "%.+?%", m => {
                var key = m.Value.Trim('%');
                return values[key];
            });
        }

        private static string ConvertEmbeddedSources(string html, string id) {
            const string pattern = "<img.+?src=\"(?<CID>cid:.+?)\".*?>";
            return Regex.Replace(html, pattern, m => {
                var cid = m.Groups["CID"].Value;
                var asset = string.Format("asset://image?cid={0}&messageId={1}",
                    Uri.EscapeDataString(cid.Split(':')[1]), id);
                return m.Value.Replace(cid, asset);
            }, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        private void SendByteStream(DataSourceRequest request, byte[] bytes) {
            var length = bytes.Length;
            var handle = Marshal.AllocHGlobal(length);
            Marshal.Copy(bytes, 0, handle, length);

            SendResponse(request, new DataSourceResponse {
                Buffer = handle,
                MimeType = "text/html",
                Size = Convert.ToUInt32(length)
            });

            Marshal.FreeHGlobal(handle);
        }

        private static async Task<byte[]> LoadMessageContentAsync(Int64 id) {
            using (var database = new DatabaseContext()) {
                var messages = await database.MimeMessages
                    .Where(x => x.MessageId == id)
                    .ToArrayAsync();

                return messages.Length > 0 ? messages[0].Data : new byte[0];
            }
        }
    }
}