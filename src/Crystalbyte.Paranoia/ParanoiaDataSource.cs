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
                    await SendMessageResponseAsync(request);
                    return;
                }

                if (Regex.IsMatch(request.Path, "message/new")) {
                    SendBlankCompositionResponse(request);
                    return;
                }

                if (Regex.IsMatch(request.Path, "message/reply")) {
                    await SendQuotedCompositionResponseAsync(request);
                    return;
                }

                if (Regex.IsMatch(request.Path, "message/forward")) {
                    await SendQuotedCompositionResponseAsync(request);
                    return;
                }

                if (Regex.IsMatch(request.Path, "file?path=.+")) {
                    SendFileCompositionResponse(request);
                    return;
                }

                if (Regex.IsMatch(request.Path, "smtp-request/[0-9]+")) {
                    await SendSmtpResponseAsync(request);
                    return;
                }

                SendResponse(request, DataSourceResponse.Empty);
            } catch (Exception ex) {
                Logger.Error(ex);
            }
        }
     
        private async Task SendSmtpResponseAsync(DataSourceRequest request) {
            var id = long.Parse(request.Path.Split('/')[1]);

            using (var database = new DatabaseContext()) {
                var smtp = await database.SmtpRequests.FindAsync(id);
                if (smtp == null) {
                    throw new MessageNotFoundException(id);
                }

                SendHtmlResponse(request, Encoding.UTF8.GetBytes(smtp.Mime));
            }
        }

        private async Task SendMessageResponseAsync(DataSourceRequest request) {
            var id = long.Parse(request.Path.Split('/')[1]);
            var arguments = request.Url.PathAndQuery.ToPageArguments();

            var mime = await LoadMessageAsync(id);
            var reader = new MailMessageReader(mime);
            var text = GetSupportedBody(reader);

            if (string.IsNullOrWhiteSpace(text)) {
                SendResponse(request, DataSourceResponse.Empty);
                return;
            }

            const string key = "blockExternals";
            var blockExternals = !arguments.ContainsKey(key) || bool.Parse(arguments[key]);

            text = NormalizeHtml(text);
            text = ConvertEmbeddedSources(text, id);
            text = RemoveJavaScript(text);
            text = InjectParanoiaScripts(text);

            if (blockExternals) {
                text = RemoveExternalSources(text);
            }

            var bytes = Encoding.UTF8.GetBytes(text);
            SendHtmlResponse(request, bytes);
        }

        private static string InjectParanoiaScripts(string view) {
            var document = new HtmlDocument();
            document.LoadHtml(view);

            var body = document.DocumentNode.SelectSingleNode("//body");

            var jquery = document.CreateElement("script");
            jquery.Attributes.Add("type", "text/javascript");
            jquery.Attributes.Add("src", "Resources/jquery/jquery-2.1.1.min.js");
            body.AppendChild(jquery);

            const string url = "pack://application:,,,/Resources/inspection.js";
            var script = Application.GetResourceStream(new Uri(url));
            if (script == null) {
                throw new ResourceNotFoundException(url);
            }

            var redirect = document.CreateElement("script");
            redirect.Attributes.Add("type", "text/javascript");
            redirect.AppendChild(document.CreateTextNode(script.Stream.ToUtf8String()));
            body.AppendChild(redirect);
            return document.DocumentNode.WriteTo();
        }

        private static string RemoveExternalSources(string content) {
            const string pattern = "(\"|&quot;|')(?<URL>http(s){0,1}://.+?)(\"|&quot;|')";
            content = Regex.Replace(content, pattern, m => {
                var resource = m.Groups["URL"].Value;
                return m.Value.Replace(resource, "");
            }, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return content;
        }

        private void SendFileCompositionResponse(DataSourceRequest request) {
            const string key = "path";
            var arguments = request.Path.ToPageArguments();
            if (!arguments.ContainsKey(key)) {
                throw new KeyNotFoundException(key);
            }

            var path = arguments[key];
            var filename = Uri.UnescapeDataString(path);

            var bytes = LoadMessageBytes(new FileInfo(filename));
            var reader = new MailMessageReader(bytes);

            var text = GetSupportedBody(reader);
            text = DropBodyStyles(text);
            text = NormalizeHtml(text);
            text = RemoveJavaScript(text);
            text = ConvertEmbeddedSources(text, new FileInfo(path));

            SendHtmlResponse(request, Encoding.UTF8.GetBytes(text));
        }

        private static string GetSupportedBody(MailMessageReader reader) {
            string text;
            var html = reader.FindFirstHtmlVersion();
            if (html == null) {
                var plain = reader.FindFirstPlainTextVersion();
                text = plain != null
                    ? FormatPlainText(reader.Headers.Subject, plain.GetBodyAsText())
                    : string.Empty;

            } else {
                text = html.GetBodyAsText();
            }

            return text;
        }

        private async Task SendQuotedCompositionResponseAsync(DataSourceRequest request) {
            var variables = new Dictionary<string, string> {
                {"header", string.Empty},
                {"culture", CultureInfo.CurrentUICulture.TwoLetterISOLanguageName},
                {"ckeditor_theme", string.Compare(Settings.Default.Theme, "light", StringComparison.InvariantCultureIgnoreCase) == 0 ? "moono" : "moono-dark"}
            };

            long messageId;
            var arguments = request.Url.OriginalString.ToPageArguments();
            if (arguments.ContainsKey("id") && long.TryParse(arguments["id"], out messageId)) {

                var message = await LoadMessageAsync(messageId);
                var reader = new MailMessageReader(message);

                var text = GetSupportedBody(reader);
                text = DropBodyStyles(text);
                text = ConvertEmbeddedSources(text, messageId);
                text = string.Format("<hr style=\"margin:20px 0px;\"/>{0}", text);
                variables.Add("quote", text);
            }

            if (!variables.Keys.Contains("quote"))
                variables.Add("quote", string.Empty);

            var html = GenerateEditorHtml(variables);
            var bytes = Encoding.UTF8.GetBytes(html);
            SendHtmlResponse(request, bytes);
        }

        private static string DropBodyStyles(string text) {
            var document = new HtmlDocument();
            document.LoadHtml(text);

            var nodes = document.DocumentNode.SelectNodes("//style");
            if (nodes != null) {
                nodes.ForEach(x => x.Remove());
            }
            
            return document.DocumentNode.WriteTo();
        }

        private void SendBlankCompositionResponse(DataSourceRequest request) {
            var variables = new Dictionary<string, string> {
                {"quote", string.Empty},
                {"header", string.Empty},
                {"culture", CultureInfo.CurrentUICulture.TwoLetterISOLanguageName},
                {"ckeditor_theme", string.Compare(Settings.Default.Theme, "light", StringComparison.InvariantCultureIgnoreCase) == 0 ? "moono" : "moono-dark"}
            };

            var html = GenerateEditorHtml(variables);
            var bytes = Encoding.UTF8.GetBytes(html);
            SendHtmlResponse(request, bytes);
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

        /// <summary>
        /// This method will attempt to repair any malformed html and adjust the charset to UTF8.
        /// </summary>
        /// <param name="text">Source text to be normalized.</param>
        /// <returns>Normalized HTML document.</returns>
        private static string NormalizeHtml(string text) {
            var document = new HtmlDocument { OptionFixNestedTags = true };
            document.LoadHtml(text);

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
            var metaCharsetNode = nodes == null ? null : nodes.FirstOrDefault(x => {
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

            // Drop previous charset tags, since all bytes have been converted to UTF-8.
            if (metaCharsetNode != null) {
                metaCharsetNode.Remove();
            }

            // Add UTF-8 charset meta tag.
            var meta = document.CreateElement("meta");

            meta.Attributes.Add(charset, "utf-8");
            head.AppendChild(meta);
            return document.DocumentNode.WriteTo();
        }

        private static string FormatPlainText(string subject, string plain) {
            const string url = "/Resources/plain-text.template.html";
            var info = Application.GetResourceStream(new Uri(url, UriKind.RelativeOrAbsolute));
            if (info == null) {
                var message = string.Format(Resources.ResourceNotFoundException, url, typeof(App).Assembly.FullName);
                throw new NullReferenceException(message);
            }

            var wrapper = info.Stream.ToUtf8String();
            var values = new Dictionary<string, string> {
                {"content", plain},
                {"subject", subject},
            };

            return Regex.Replace(wrapper, "%.+?%", m => {
                var key = m.Value.Trim('%');
                return values[key];
            });
        }

        private static string ConvertEmbeddedSources(string html, FileSystemInfo info) {
            const string pattern = "<img.+?src=\"(?<CID>cid:.+?)\".*?>";
            return Regex.Replace(html, pattern, m => {
                var cid = m.Groups["CID"].Value;
                var asset = string.Format("asset://image?cid={0}&path={1}",
                    Uri.EscapeDataString(cid.Split(':')[1]), Uri.EscapeDataString(info.FullName));
                return m.Value.Replace(cid, asset);
            }, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        private static string ConvertEmbeddedSources(string html, long id) {
            const string pattern = "<img.+?src=\"(?<CID>cid:.+?)\".*?>";
            return Regex.Replace(html, pattern, m => {
                var cid = m.Groups["CID"].Value;
                var asset = string.Format("asset://image?cid={0}&messageId={1}",
                    Uri.EscapeDataString(cid.Split(':')[1]), id);
                return m.Value.Replace(cid, asset);
            }, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        private void SendHtmlResponse(DataSourceRequest request, byte[] bytes) {
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

        private static byte[] LoadMessageBytes(FileSystemInfo file) {
            return File.ReadAllBytes(file.FullName);
        }

        private static async Task<byte[]> LoadMessageAsync(Int64 id) {
            using (var database = new DatabaseContext()) {
                var messages = await database.MimeMessages
                    .Where(x => x.MessageId == id)
                    .ToArrayAsync();

                return messages.Length > 0 ? messages[0].Data : new byte[0];
            }
        }

        private static string RemoveJavaScript(string content) {
            const string pattern = "<script.+?>.*?</script>|<script.+?/>";
            return Regex.Replace(content, pattern, string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        }
    }
}