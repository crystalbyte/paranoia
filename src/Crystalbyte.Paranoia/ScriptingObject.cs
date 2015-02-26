using System;
using System.Windows;
using Crystalbyte.Paranoia.UI;
using Newtonsoft.Json;

namespace Crystalbyte.Paranoia {
    internal sealed class ScriptingObject {
        private readonly HtmlEditor _editor;

        public ScriptingObject(HtmlEditor editor) {
            _editor = editor;
        }

        public void NotifyContentReady() {
            Application.Current.Dispatcher.Invoke(() => _editor.OnContentReady());
        }

        public void NotifySelectionChanged(string json) {
            if (string.IsNullOrEmpty(json) || string.Compare("null", json, StringComparison.OrdinalIgnoreCase) == 0) {
                return;
            }
            var range = JsonConvert.DeserializeObject<TextRange>(json);
            Application.Current.Dispatcher.Invoke(async () => {
                _editor.Selection = range;
                await _editor.InvalidateCommandsAsync();
            });
        }

        public void NotifyTextChanged(string source, string delta) {
            Application.Current.Dispatcher.Invoke(async () => {
                _editor.Content = await _editor.GetHtmlAsync();
                await _editor.UpdateSelectionAsync();
            });
        }
    }
}
