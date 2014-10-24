using System.Reflection;
using System.Windows;
using Awesomium.Core;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Crystalbyte.Paranoia {
    public class ResourceInterceptor : IResourceInterceptor {
        public bool OnFilterNavigation(NavigationRequest request) {
            return false;
        }
        internal void SetCurrentMessage() {
        }

        public ResourceResponse OnRequest(ResourceRequest request) {

            var arguments = request.Url.OriginalString.ToPageArguments();

            if (request.Url.Scheme == "http" || request.Url.Scheme == "https") {
                bool suppress;
                if (!arguments.ContainsKey("suppressExternals")
                    || !bool.TryParse(arguments["suppressExternals"], out suppress)) {
                    return GetPlaceHolderResponse();
                }

                return !suppress ? null : GetPlaceHolderResponse();
            }

            if (request.Url.Scheme != "asset")
                return null;

            long messageId;
            if (arguments.ContainsKey("messageId") && long.TryParse(arguments["messageId"], out messageId)) {
                if (arguments.ContainsKey("cid")) {

                    var attachment = GetAttachmentBytes(arguments["cid"], messageId);
                    if (attachment != null) {
                        return BytesToResourceResponce(attachment, "image");
                    }
                }
            }

            //asset://tempImage/
            var image = request.Url.AbsolutePath.StartsWith("/") ? request.Url.AbsolutePath.Remove(0, 1) : request.Url.AbsolutePath;
            if (File.Exists(image)) {
                return ResourceResponse.Create(image);
            }

            Debug.WriteLine(request.Url.AbsolutePath);

            // TODO: add more resources if needed
            if (!request.Url.AbsolutePath.EndsWith(".js") && !request.Url.AbsolutePath.EndsWith(".css") &&
                !request.Url.AbsolutePath.EndsWith(".png")) return null;

            var basePath = new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName;
            var file = basePath + request.Url.AbsolutePath.Replace("/message", string.Empty);
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
                    var attachment = attachments.FirstOrDefault(x => x.ContentId == Uri.UnescapeDataString(cid));
                    if (attachment != null)
                        return attachment.Body;
                }
            }

            return null;
        }

        private static ResourceResponse BytesToResourceResponce(byte[] bytes, string mediaType) {
            var buffer = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, buffer, bytes.Length);

            var response = ResourceResponse.Create((uint)bytes.Length, buffer, mediaType);

            Marshal.FreeHGlobal(buffer);
            return response;
        }

        private static ResourceResponse GetPlaceHolderResponse() {
            const string path = "pack://application:,,,/assets/image.placeholder.png";
            var info = Application.GetResourceStream(new Uri(path));
            if (info == null) {
                throw new Exception(path);
            }
            using (var stream = info.Stream) {
                using (var memStream = new MemoryStream()) {
                    stream.CopyTo(memStream);
                    var bytes = memStream.ToArray();
                    return BytesToResourceResponce(bytes, "image");
                }
            }
        }
    }
}

