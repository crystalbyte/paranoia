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

        public ResourceResponse OnRequest(ResourceRequest request) {
            if (request.Url.Scheme != "asset")
                return null;

            //TODO improve this
            var arguments = request.Url.OriginalString.ToPageArguments();
            long messageId;
            if (arguments.ContainsKey("messageId") && long.TryParse(arguments["messageId"], out messageId)) {
                if (arguments.ContainsKey("cid")) {

                    var attachment = GetAttachmentBytes(arguments["cid"], messageId);
                    if (attachment != null) {
                        var buffer = Marshal.AllocHGlobal(attachment.Length);
                        Marshal.Copy(attachment, 0, buffer, attachment.Length);

                        var response = ResourceResponse.Create((uint)attachment.Length, buffer, "image");

                        Marshal.FreeHGlobal(buffer);
                        return response;
                    }
                }
            }

            //asset://tempImage/
            var image = request.Url.AbsolutePath.StartsWith("/") ? request.Url.AbsolutePath.Remove(0, 1) : request.Url.AbsolutePath;
            if (File.Exists(image)) {
                return ResourceResponse.Create(image);
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

        internal static byte[] GetAttachmentBytes(string cid, long messageId) {
            using (var database = new DatabaseContext()) {
                var message = database.MimeMessages.FirstOrDefault(x => x.MessageId == messageId);

                if (message != null) {
                    var reader = new MailMessageReader(message.Data);
                    var attachments = reader.FindAllAttachments();
                    return attachments.FirstOrDefault(x => x.ContentId == Uri.UnescapeDataString(cid)).Body;
                }
            }

            return null;
        }
    }
}

