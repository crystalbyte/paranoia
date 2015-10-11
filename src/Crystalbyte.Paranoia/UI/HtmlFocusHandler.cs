using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;

namespace Crystalbyte.Paranoia.UI {
    internal sealed class HtmlFocusHandler : IFocusHandler {

        private HtmlViewer _viewer;

        public HtmlFocusHandler(HtmlViewer viewer) {
            _viewer = viewer;
        }

        public void OnGotFocus() {
            // Nada ...
        }

        public bool OnSetFocus(CefFocusSource source) {
            if (source == CefFocusSource.FocusSourceNavigation) {
                return _viewer.IsNavigationFocusDisabled;
            }

            return false;
        }

        public void OnTakeFocus(bool next) {
            // Nada ...
        }
    }
}
