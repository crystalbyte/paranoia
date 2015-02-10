using System;
using CefSharp;

namespace Crystalbyte.Paranoia {
    internal sealed class SchemeHandlerFactory<T> : ISchemeHandlerFactory where T : ISchemeHandler {
        public ISchemeHandler Create() {
            return Activator.CreateInstance(typeof(T)) as ISchemeHandler;
        }
    }
}
