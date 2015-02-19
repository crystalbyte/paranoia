using System;
using System.Diagnostics;
using System.Windows;
using Crystalbyte.Paranoia.UI;
using Newtonsoft.Json;
using NLog;

namespace Crystalbyte.Paranoia {
    internal sealed class ScriptingObject {
        private readonly HtmlEditor _editor;

        public ScriptingObject(HtmlEditor editor) {
            _editor = editor;
        }

        public void NotifySelectionChanged(string json) {
            if (string.IsNullOrEmpty(json) || string.Compare("null", json, StringComparison.OrdinalIgnoreCase) == 0) {
                return;
            }
            var range = JsonConvert.DeserializeObject<TextRange>(json);
            Application.Current.Dispatcher.Invoke(() => {
                _editor.Selection = range;
            });
        }

        public void NotifyTextChanged(string source, string json) {
            Application.Current.Dispatcher.Invoke(async () => {
                _editor.Content = await _editor.GetHtmlAsync();
                await _editor.UpdateSelectionAsync();
            });
        }
    }
}
