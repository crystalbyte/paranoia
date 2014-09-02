using Awesomium.Core;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Crystalbyte.Paranoia {
    public class ResourceInterceptor : IResourceInterceptor {
        public bool OnFilterNavigation(NavigationRequest request) {
            return false;
        }
        internal void SetCurrentMessage() { 
        }

        private long _lastId = -1;
        public ResourceResponse OnRequest(ResourceRequest request) {
            if (request.Url.Scheme != "asset")
                return null;


            //TODO improve this
            if (_lastId != -1 && request.Url.Segments[1] == "cid/") {
                using (var database = new DatabaseContext()) {
                    var message = database.MimeMessages.FirstOrDefault(x => x.MessageId == _lastId);

                    if (message != null) {
                        var reader = new MailMessageReader(Encoding.UTF8.GetBytes(message.Data));
                        var attachments = reader.FindAllAttachments();

                        var attachment = attachments.FirstOrDefault(x => x.ContentId == request.Url.Segments[2]);
                        if (attachment != null) {
                            var buffer = Marshal.AllocHGlobal(attachment.Body.Length);
                            Marshal.Copy(attachment.Body, 0, buffer, attachment.Body.Length);

                            var response = ResourceResponse.Create((uint)attachment.Body.Length, buffer, "image");

                            Marshal.FreeHGlobal(buffer);
                            return response;
                        }
                    }
                }
            }

            if (!long.TryParse(request.Url.Segments[2], out _lastId)) {
                _lastId = -1;
            }

            Debug.WriteLine(request.Url.AbsolutePath);

            //TODO add more resources if needed
            if (!request.Url.AbsolutePath.EndsWith(".js") && !request.Url.AbsolutePath.EndsWith(".css") &&
                !request.Url.AbsolutePath.EndsWith(".png")) return null;
            var file = Environment.CurrentDirectory + request.Url.AbsolutePath.Replace("/message", string.Empty);
            if (File.Exists(file))
                return ResourceResponse.Create(file);
            throw new Exception("resource could not be resolved");
        }
    }
}
