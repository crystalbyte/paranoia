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

namespace Crystalbyte.Paranoia.UI {
    internal sealed class ParanoiaDataSource : DataSource {
        protected async override void OnRequest(DataSourceRequest request) {

            if (Regex.IsMatch(request.Path, "message/[0-9]+")) {
                var id = request.Path.Split('/')[1];
                await SendHtmlResponseAsync(request, id);
                return;
            }

            SendResponse(request, DataSourceResponse.Empty);
        }

        private async Task SendHtmlResponseAsync(DataSourceRequest request, string id) {
            var mime = await LoadMessageContentAsync(Int64.Parse(id));

            var mimeBytes = Encoding.UTF8.GetBytes(mime);
            var message = new MailMessage(mimeBytes);
            
            var content = message.FindFirstHtmlVersion() ?? message.FindFirstPlainTextVersion();
            if (content == null) {
                SendResponse(request, DataSourceResponse.Empty);
                return;
            }

            var length = content.Body.Length;
            var handle = Marshal.AllocHGlobal(length);
            Marshal.Copy(content.Body, 0, handle, length);

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
