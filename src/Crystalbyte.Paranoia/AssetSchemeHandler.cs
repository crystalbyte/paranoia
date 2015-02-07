using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CefSharp;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using dotless.Core;
using HtmlAgilityPack;
using NLog;

namespace Crystalbyte.Paranoia {
    internal sealed class AssetSchemeHandler : ISchemeHandler {

        #region Private Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion

        public bool ProcessRequestAsync(IRequest request, ISchemeHandlerResponse response,
            OnRequestCompletedHandler requestCompletedCallback) {
            try {
                if (Regex.IsMatch(request.Url, "message/[0-9]+")) {
                    Task.Run(() => {
                        ComposeMessageResponse(request, response);
                        requestCompletedCallback();
                    });

                    return true;
                }

                if (Regex.IsMatch(request.Url, "message/new")) {
                    Task.Run(() => {
                        // Implicitly captured closure is fine.
                        ComposeBlankCompositionResponse(response);
                        requestCompletedCallback();
                    });

                    return true;
                }

                if (Regex.IsMatch(request.Url, "message/reply")) {
                    Task.Run(() => {
                        // Implicitly captured closure is fine.
                        ComposeQuotedCompositionResponse(request, response);
                        requestCompletedCallback();
                    });

                    return true;
                }

                if (Regex.IsMatch(request.Url, "message/forward")) {
                    Task.Run(() => {
                        ComposeQuotedCompositionResponse(request, response);
                        requestCompletedCallback();
                    });

                    return true;
                }

                if (Regex.IsMatch(request.Url, "file?path=.+")) {
                    Task.Run(() => {
                        ComposeFileCompositionResponse(request, response);
                        requestCompletedCallback();
                    });

                    return true;
                }

                if (Regex.IsMatch(request.Url, "composition/[0-9]+")) {
                    Task.Run(() => {
                        ComposeCompositionResponse(request, response);
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

        private static void ComposeCompositionResponse(IRequest request, ISchemeHandlerResponse response) {
            var uri = new Uri(request.Url);
            var id = long.Parse(uri.LocalPath.Split('/')[1]);

            using (var database = new DatabaseContext()) {
                var smtp = database.SmtpRequests.Find(id);
                if (smtp == null) {
                    throw new MessageNotFoundException(id);
                }

                var bytes = Encoding.UTF8.GetBytes(smtp.Mime);
                ComposeHtmlResponse(response, bytes);
            }
        }

        private static void ComposeQuotedCompositionResponse(IRequest request, ISchemeHandlerResponse response) {
            var variables = new Dictionary<string, string>
            {
                {"header", string.Empty},
                {"culture", CultureInfo.CurrentUICulture.TwoLetterISOLanguageName},
                {
                    "ckeditor_theme",
                    string.Compare(Settings.Default.Theme, "light", StringComparison.InvariantCultureIgnoreCase) == 0
                        ? "moono"
                        : "moono-dark"
                }
            };

            long messageId;
            var uri = new Uri(request.Url);
            var arguments = uri.OriginalString.ToPageArguments();
            if (arguments.ContainsKey("id") && long.TryParse(arguments["id"], out messageId)) {

                var message = GetMessageBytes(messageId);
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
            ComposeHtmlResponse(response, bytes);
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

            var text = GetSupportedBody(reader);
            text = DropBodyStyles(text);
            text = NormalizeHtml(text);
            text = RemoveJavaScript(text);
            text = ConvertEmbeddedSources(text, new FileInfo(path));

            var b = Encoding.UTF8.GetBytes(text);
            ComposeHtmlResponse(response, b);
        }

        private static string RemoveJavaScript(string content) {
            const string pattern = "<script.+?>.*?</script>|<script.+?/>";
            return Regex.Replace(content, pattern, string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase);
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

        private static string RemoveExternalSources(string content) {
            const string pattern = "(\"|&quot;|')(?<URL>http(s){0,1}://.+?)(\"|&quot;|')";
            content = Regex.Replace(content, pattern, m => {
                var resource = m.Groups["URL"].Value;
                return m.Value.Replace(resource, "");
            },
                RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return content;
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

        private static void ComposeMessageResponse(IRequest request, ISchemeHandlerResponse response) {
            var uri = new Uri(request.Url);
            var id = long.Parse(uri.Segments[1]);

            var arguments = uri.PathAndQuery.ToPageArguments();

            var mime = GetMessageBytes(id);
            var reader = new MailMessageReader(mime);
            var text = GetSupportedBody(reader);

            if (string.IsNullOrWhiteSpace(text)) {
                ComposeHtmlResponse(response, null);
                return;
            }

            const string key = "blockExternals";
            var blockExternals = !arguments.ContainsKey(key) || bool.Parse(arguments[key]);

            text = NormalizeHtml(text);
            text = ConvertEmbeddedSources(text, id);
            text = RemoveJavaScript(text);
            //text = InjectParanoiaScripts(text);

            if (blockExternals) {
                text = RemoveExternalSources(text);
            }

            var bytes = Encoding.UTF8.GetBytes(text);
            ComposeHtmlResponse(response, bytes);
        }

        private static void ComposeBlankCompositionResponse(ISchemeHandlerResponse response) {
            var variables = new Dictionary<string, string>
            {
                {"quote", string.Empty},
                {"header", string.Empty},
                {"culture", CultureInfo.CurrentUICulture.TwoLetterISOLanguageName},
                {
                    "ckeditor_theme",
                    string.Compare(Settings.Default.Theme, "light", StringComparison.InvariantCultureIgnoreCase) == 0
                        ? "moono"
                        : "moono-dark"
                }
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

            // Find first existing style tag.
            var style = document.DocumentNode.SelectSingleNode("//head/style");

            // In order to minmize visual issues a wellformed document must be present.
            // The document should contain exactly one head and one body.
            // The base style sheet must be placed inside the head as the fist style element.
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

            // Check for any existing charset settings.
            var nodes = document.DocumentNode.SelectNodes("//head/meta");
            var metaCharsetNode = nodes == null
                ? null
                : nodes.FirstOrDefault(x => {
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

            // Add CSS base style.
            var css = GetCssResource("/Resources/html.inspection.less");
            var node = document.CreateTextNode(css);
            var baseStyle = document.CreateElement("style");
            baseStyle.AppendChild(node);
            head.InsertBefore(baseStyle, style);

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

        public static string GetCssResource(string path) {
            var uri = new Uri(path, UriKind.Relative);
            var info = Application.GetResourceStream(uri);
            if (info == null) {
                var error = string.Format(Resources.ResourceNotFoundException, uri, typeof(App).Assembly.FullName);
                throw new Exception(error);
            }

            string less;
            const string pattern = "%.+?%";
            using (var reader = new StreamReader(info.Stream)) {
                var text = reader.ReadToEnd();
                less = Regex.Replace(text, pattern, m => {
                    var key = m.Value.Trim('%');
                    var dictionary =
                        Application.Current.Resources.MergedDictionaries.FirstOrDefault(
                            x => x.Contains(key)) ?? Application.Current.Resources;

                    var resource = dictionary[key];
                    if (resource is SolidColorBrush) {
                        return string.Format("#{0}", (resource as SolidColorBrush).Color.ToString().Substring(3));
                    }

                    return resource != null ? resource.ToString() : "fuchsia";
                });
            }

            return Less.Parse(less);
        }

        private static string ConvertEmbeddedSources(string html, FileSystemInfo info) {
            const string pattern = "<img.+?src=\"(?<CID>cid:.+?)\".*?>";
            return Regex.Replace(html, pattern, m => {
                var cid = m.Groups["CID"].Value;
                var asset = string.Format("asset://image?cid={0}&path={1}",
                    Uri.EscapeDataString(cid.Split(':')[1]),
                    Uri.EscapeDataString(info.FullName));
                return m.Value.Replace(cid, asset);
            },
                RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        private static string ConvertEmbeddedSources(string html, long id) {
            const string pattern = "<img.+?src=\"(?<CID>cid:.+?)\".*?>";
            return Regex.Replace(html, pattern, m => {
                var cid = m.Groups["CID"].Value;
                var asset = string.Format("asset://image?cid={0}&messageId={1}",
                    Uri.EscapeDataString(cid.Split(':')[1]), id);
                return m.Value.Replace(cid, asset);
            },
                RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        private static byte[] GetFileBytes(FileSystemInfo file) {
            return File.ReadAllBytes(file.FullName);
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
            response.StatusCode = 200;
            response.ContentLength = bytes.Length;
            response.ResponseStream = new MemoryStream(bytes);
        }
    }
}
