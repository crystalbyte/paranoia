using System;
using System.Collections.Generic;

namespace Crystalbyte.Paranoia.UI {
    internal static class HtmlStorage {

        #region Private Fields

        private static readonly Dictionary<Guid, string> Sources = new Dictionary<Guid, string>();

        #endregion

        #region Methods

        public static void Push(Guid guid, string source) {
            Sources.Add(guid, source);
        }
        public static string Pull(Guid guid) {
            var html = Sources[guid];
            Sources.Remove(guid);
            return html;
        }

        #endregion
    }
}
