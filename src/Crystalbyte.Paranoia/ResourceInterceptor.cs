using Awesomium.Core;
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

        public ResourceResponse OnRequest(ResourceRequest request) {
            if (request.Url.Scheme != "asset")
                return null;

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
