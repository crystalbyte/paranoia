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
using System.Collections;
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

                SendResponse(request, DataSourceResponse.Empty);
            }
            catch (Exception ex) {
                //TODO: Return 793 - Zombie Apocalypse
                Logger.Error(ex);
            }
        }

        private void SendComposeAsNewResponse(DataSourceRequest request) {
            var variables = new Dictionary<string, string>() {
                {"content", string.Empty},
                {"current_dir", string.Format("file://127.0.0.1/{0}", Environment.CurrentDirectory.Replace(@"\", "/").Replace(":","$")) }
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

            string html = string.Empty;
            const string pattern = "%.+?%";
            using (var reader = new StreamReader(info.Stream)) {

                var text = reader.ReadToEnd();

                html = Regex.Replace(text, pattern, m => {
                    var key = m.Value.Trim('%').ToLower();
                    if (variables.ContainsKey(key)) {
                        return variables[key];
                    }

                    return string.Empty;
                }, RegexOptions.IgnoreCase);
            }

            return html;
        }

        private async Task SendHtmlResponseAsync(DataSourceRequest request, string id, bool isReadOnly = true) {
            var mime = await LoadMessageContentAsync(Int64.Parse(id));

            var mimeBytes = Encoding.UTF8.GetBytes(mime);
            var message = new MailMessage(mimeBytes);

            var content = message.FindFirstHtmlVersion() ?? message.FindFirstPlainTextVersion();
            if (content == null) {
                SendResponse(request, DataSourceResponse.Empty);
                return;
            }

            SendByteStream(request, content.Body);
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