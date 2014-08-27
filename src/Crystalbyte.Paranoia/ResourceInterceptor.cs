using Awesomium.Core;
using Crystalbyte.Paranoia.Data;
using Crystalbyte.Paranoia.Mail;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    public class ResourceInterceptor : IResourceInterceptor {
        public bool OnFilterNavigation(NavigationRequest request) {
            return false;
        }

        private long _lastID = -1;
        public ResourceResponse OnRequest(ResourceRequest request) {
            if (request.Url.Scheme != "asset")
                return null;


            //TODO improve this
            if (_lastID != -1 && request.Url.Segments[1] == "cid/") {
                using (var database = new DatabaseContext()) {
                    var message = database.MimeMessages
                        .Where(x => x.MessageId == _lastID).FirstOrDefault();

                    if (message != null) {
                        var reader = new MailMessageReader(Encoding.UTF8.GetBytes(message.Data));
                        var attachments = reader.FindAllAttachments();

                        var attachment = attachments.Where(x => x.ContentId == request.Url.Segments[2]).FirstOrDefault();
                        if (attachment != null) {
                            var file = Path.GetTempFileName();
                            attachment.Save(new FileInfo(file));
                            return ResourceResponse.Create(file);
                        }
                    }
                }
            }

            if (!long.TryParse(request.Url.Segments[2], out _lastID)) {
                _lastID = -1;
            }

            Debug.WriteLine(request.Url.AbsolutePath);

            //TODO add more resources if needed
            if (request.Url.AbsolutePath.EndsWith(".js")
                || request.Url.AbsolutePath.EndsWith(".css")
                || request.Url.AbsolutePath.EndsWith(".png")) {
                var file = Environment.CurrentDirectory + request.Url.AbsolutePath.Replace("/message", string.Empty);
                if (File.Exists(file))
                    return ResourceResponse.Create(file);
                else {
                    throw new Exception("resource could not be resolved");
                }
            }

            return null;
        }
    }
}
