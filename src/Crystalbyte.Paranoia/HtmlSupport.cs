using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using dotless.Core;
using HtmlAgilityPack;

namespace Crystalbyte.Paranoia {
    internal static class HtmlSupport {

        /// <summary>
        /// Finds the best suitable body. Html is favored before plain text.
        /// </summary>
        /// <param name="reader">The reader containing the relevant message.</param>
        /// <returns>Returns the found body text.</returns>
        public static string FindBestSupportedBody(MailMessageReader reader) {
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

        /// <summary>
        /// Loads the template for the html editor.
        /// </summary>
        /// <param name="variables">The dictionary containing template variables.</param>
        /// <returns>Returns the generated html source.</returns>
        public static string GetEditorTemplate(IDictionary<string, string> variables) {
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
        /// In addition all script tags will be stripped.
        /// </summary>
        /// <param name="text">Source text to be normalized.</param>
        /// <returns>Normalized HTML document.</returns>
        public static string PrepareHtmlForInspection(string text) {
            var document = new HtmlDocument { OptionFixNestedTags = true };
            document.LoadHtml(text);

            // Drop all script tags.
            var scripts = document.DocumentNode.SelectNodes("//script");
            if (scripts != null) {
                scripts.ForEach(x => x.Remove());
            }

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

            // In order to minimize visual issues a wellformed document must be present.
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

        public static string FormatPlainText(string subject, string plain) {
            const string url = "/Resources/text.template.html";
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

        public static string ModifyEmbeddedParts(string html, FileSystemInfo info) {
            const string pattern = "<img.+?src=\"(?<CID>cid:.+?)\".*?>";
            return Regex.Replace(html, pattern, m => {
                var cid = m.Groups["CID"].Value;
                var asset = string.Format("message:///part?cid={0}&path={1}",
                    Uri.EscapeDataString(cid.Split(':')[1]),
                    Uri.EscapeDataString(info.FullName));
                return m.Value.Replace(cid, asset);
            }, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public static string ModifyEmbeddedParts(string html, long id) {
            const string pattern = "<img.+?src=\"(?<CID>cid:.+?)\".*?>";
            return Regex.Replace(html, pattern, m => {
                var cid = m.Groups["CID"].Value;
                var asset = string.Format("message:///part?cid={0}&messageId={1}",
                    Uri.EscapeDataString(cid.Split(':')[1]), id);
                return m.Value.Replace(cid, asset);
            }, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        internal static string InsertQuoteSeparator(string text) {
            return string.Format("<hr style=\"margin:20px 0px;\"/>{0}{1}", Environment.NewLine, text);
        }

        internal static string SuppressGlobalStyles(string text) {
            // TODO: replace any "body {}" styles by renaming.
            return text;
        }
    }
}
