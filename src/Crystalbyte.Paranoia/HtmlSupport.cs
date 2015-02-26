﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
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
                text = string.Format("<pre>{0}</pre>", plain.GetBodyAsText());

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
            var uri = new Uri("/Resources/composition.html", UriKind.Relative);
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
            var quote = new HtmlDocument { OptionFixNestedTags = true };
            quote.LoadHtml(text);

            // Drop all script tags.
            var scripts = quote.DocumentNode.SelectNodes("//script");
            if (scripts != null) {
                scripts.ForEach(x => x.Remove());
            }

            // Drop all meta tags.
            var metas = quote.DocumentNode.SelectNodes("//meta");
            if (metas != null) {
                metas.ForEach(x => x.Remove());
            }

            const string name = "/Resources/inspection.html";
            var info = Application.GetResourceStream(new Uri(name, UriKind.Relative));
            if (info == null) {
                var m = string.Format(Resources.ResourceNotFoundException, name, typeof(Application).Assembly.FullName);
                throw new ResourceNotFoundException(m);
            }

            string template;
            using (var reader = new StreamReader(info.Stream)) {
                template = reader.ReadToEnd();
            }

            var document = new HtmlDocument { OptionFixNestedTags = true };
            document.LoadHtml(template);

            var host = document.DocumentNode.SelectSingleNode("//div[@id='content-host']");
            if (host == null) {
                throw new Exception(Resources.ContentHostMissingException);
            }


            host.InnerHtml = quote.DocumentNode.WriteTo();
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
            using (var reader = new StreamReader(info.Stream)) {
                less = reader.ReadToEnd();
            }

            return Less.Parse(less);
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
    }
}
