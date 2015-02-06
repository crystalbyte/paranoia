using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;

namespace Crystalbyte.Paranoia {
    internal sealed class AssetSchemeHandlerFactory : ISchemeHandlerFactory {
        public ISchemeHandler Create() {
            return new AssetSchemeHandler();
        }
    }
}
