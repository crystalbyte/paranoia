using System;
using System.Runtime.InteropServices;
using System.Text;
using Awesomium.Core.Data;

namespace Crystalbyte.Paranoia.UI {
    internal sealed class HtmlDataSource : DataSource {
        protected override void OnRequest(DataSourceRequest request) {
            if (string.IsNullOrEmpty(request.Path)) {
                SendResponse(request, DataSourceResponse.Empty);
                return;
            }

            var html = HtmlStorage.Pull(new Guid(request.Path));
            var bytes = Encoding.UTF8.GetBytes(html);

            var handle = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, handle, bytes.Length);

            SendResponse(request, new DataSourceResponse {
                Buffer = handle,
                MimeType = "text/html",
                Size = Convert.ToUInt32(bytes.Length)
            });

            Marshal.FreeHGlobal(handle);
        }
    }
}
