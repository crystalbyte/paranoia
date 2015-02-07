using CefSharp;

namespace Crystalbyte.Paranoia {
    internal sealed class AssetSchemeHandlerFactory : ISchemeHandlerFactory {
        public ISchemeHandler Create() {
            return new AssetSchemeHandler();
        }
    }
}
