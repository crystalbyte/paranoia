#region Copyright Notice & Copying Permission

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

using Crystalbyte.Paranoia.UI.Converters;

#region Using Directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using Crystalbyte.Paranoia.Mail;
using Crystalbyte.Paranoia.Properties;
using CsQuery;
using dotless.Core;

#endregion

//using HtmlAgilityPack;

namespace Crystalbyte.Paranoia {
    internal static class HtmlSupport {
        /// <summary>
        ///     Finds the best suitable body. Html is favored before plain text.
        /// </summary>
        /// <param name="reader">The reader containing the relevant message.</param>
        /// <returns>Returns the found body text.</returns>
        public static string FindBestSupportedBody(MailMessageReader reader) {
            string text;
            var html = reader.FindFirstHtmlVersion();
            if (html == null) {
                var plain = reader.FindFirstPlainTextVersion();
                text = string.Format("<pre>{0}</pre>", plain.GetBodyAsText().Trim());
            }
            else {
                text = html.GetBodyAsText();
            }

            return text;
        }

        /// <summary>
        ///     Loads the template for the html editor.
        /// </summary>
        /// <param name="variables">The dictionary containing template variables.</param>
        /// <returns>Returns the generated html source.</returns>
        public static string GetEditorTemplate(IDictionary<string, string> variables) {
            var uri = new Uri("/Resources/composition.html", UriKind.Relative);
            var info = Application.GetResourceStream(uri);
            if (info == null) {
                var error = string.Format(Resources.ResourceNotFoundException, uri, typeof (App).Assembly.FullName);
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
        ///     This method will attempt to repair any malformed html and adjust the charset to UTF8.
        ///     In addition all script tags will be stripped.
        /// </summary>
        /// <param name="text">Source text to be normalized.</param>
        /// <returns>Normalized HTML document.</returns>
        public static string PrepareHtmlForInspection(string text) {
            var quote = new CQ(text);

            // Drop all script tags.
            var scripts = quote["script"];
            if (scripts != null) {
                scripts.ForEach(x => x.Remove());
            }

            // Drop all meta tags.
            var metas = quote["meta"];
            if (metas != null) {
                metas.ForEach(x => x.Remove());
            }

            // Drop all 'target' attributes in hyperlinks, these pop unwanted windows.
            var targets = quote["a[target]"];
            if (targets != null) {
                targets.ForEach(x => x.RemoveAttribute("target"));    
            }

            const string name = "/Resources/inspection.html";
            var info = Application.GetResourceStream(new Uri(name, UriKind.Relative));
            if (info == null) {
                var m = string.Format(Resources.ResourceNotFoundException, name, typeof (Application).Assembly.FullName);
                throw new ResourceNotFoundException(m);
            }

            string template;
            using (var reader = new StreamReader(info.Stream)) {
                template = reader.ReadToEnd();
            }

            var harness = new CQ(template);
            var host = harness["div#content-host"];
            if (host == null) {
                throw new Exception(Resources.ContentHostMissingException);
            }

            host.Html(quote.Render());
            return host.Render();
        }

        public static string GetCssResource(string path) {
            var uri = new Uri(path, UriKind.Relative);
            var info = Application.GetResourceStream(uri);
            if (info == null) {
                var error = string.Format(Resources.ResourceNotFoundException, uri, typeof (App).Assembly.FullName);
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
                                                },
                RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public static string ModifyEmbeddedParts(string html, long id) {
            const string pattern = "<img.+?src=\"(?<CID>cid:.+?)\".*?>";
            return Regex.Replace(html, pattern, m => {
                                                    var cid = m.Groups["CID"].Value;
                                                    var asset = string.Format("message:///part?cid={0}&messageId={1}",
                                                        Uri.EscapeDataString(cid.Split(':')[1]), id);
                                                    return m.Value.Replace(cid, asset);
                                                },
                RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }
}