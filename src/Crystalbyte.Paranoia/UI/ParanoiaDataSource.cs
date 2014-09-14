#region Using directives

using System;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Awesomium.Core.Data;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using System.Windows;
using NLog;
using Crystalbyte.Paranoia.Properties;
using System.IO;
using System.Collections.Generic;

#endregion

namespace Crystalbyte.Paranoia.UI {
    internal sealed class ParanoiaDataSource : DataSource {

        private readonly static Logger Logger = LogManager.GetCurrentClassLogger();

        protected override async void OnRequest(DataSourceRequest request) {
            try {
                if (Regex.IsMatch(request.Path, "message/[0-9]+")) {
                    var id = request.Path.Split('/')[1];
                    await SendHtmlResponseAsync(request, id);
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
            long messageID;
            var variables = new Dictionary<string, string>();
            var arguments = request.Url.OriginalString.ToPageArguments();
            if (arguments.ContainsKey("id") && long.TryParse(arguments["id"], out messageID)) {
                var bodyHtml = GetBodyHtmlFromId(messageID);
                bodyHtml = HandleImages(bodyHtml, messageID.ToString());
                //todo insert reply header
                variables.Add("content", bodyHtml);
            }

            if (!variables.Keys.Contains("content"))
                variables.Add("content", string.Empty);

            var html = GenerateEditorHtml(variables);

            var bytes = Encoding.UTF8.GetBytes(html);
            SendByteStream(request, bytes);
        }

        private string GetBodyHtmlFromId(long id) {
            using (var database = new DatabaseContext()) {
                var message = database.MimeMessages.FirstOrDefault(x => x.MessageId == id);
                if (message != null) {
                    var reader = new MailMessageReader(Encoding.UTF8.GetBytes(message.Data));
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

        private async Task SendHtmlResponseAsync(DataSourceRequest request, string id) {
            var mime = await LoadMessageContentAsync(Int64.Parse(id));

            var mimeBytes = Encoding.UTF8.GetBytes(mime);
            var message = new MailMessageReader(mimeBytes);

            var content = message.FindFirstHtmlVersion() ?? message.FindFirstPlainTextVersion();
            if (content == null) {
                SendResponse(request, DataSourceResponse.Empty);
                return;
            }
            //TODO improve this
            var bytes = Encoding.UTF8.GetBytes(HandleImages(Encoding.UTF8.GetString(content.Body), id));


            SendByteStream(request, bytes);
        }

        private string HandleImages(string body, string id) {
            const string imageTagRegexPattern = "<img.*?>(</img>){0,1}";
            const string srcPrepRegexPatter = "src=\".*?\"";

            var imageTagMatches = Regex.Matches(body, imageTagRegexPattern, RegexOptions.Singleline | RegexOptions.Compiled);

            foreach (Match match in imageTagMatches) {
                var originalSrcFile = Regex.Match(match.Value, srcPrepRegexPatter).Value;
                var srcFile = originalSrcFile.Replace("src=\"cid:", string.Empty).Replace("src=\"", string.Empty).Replace("\"", string.Empty).Replace("file:///", string.Empty);
                if (srcFile.StartsWith("http://") || srcFile.StartsWith("https://"))
                    continue;

                body = body.Replace(originalSrcFile, string.Format("src=\"asset://image?cid={0}&messageId={1}\"", Uri.EscapeDataString(srcFile), id));
            }
            return body;
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

        private static async Task<string> LoadMessageContentAsync(Int64 id) {
            using (var database = new DatabaseContext()) {
                var messages = await database.MimeMessages
                    .Where(x => x.MessageId == id)
                    .ToArrayAsync();

                return messages.Length > 0 ? messages[0].Data : string.Empty;
            }
        }
    }
}