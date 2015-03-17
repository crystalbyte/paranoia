#region Copyright Notice & Copying Permission

// Copyright 2014 - 2015
// 
// Alexander Wieser <alexander.wieser@crystalbyte.de>
// Sebastian Thobe
// Marvin Schluch
// 
// This file is part of Crystalbyte.Paranoia
// 
// Crystalbyte.Paranoia is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License.
// 
// Foobar is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Using Directives

using System;
using System.Windows;
using Crystalbyte.Paranoia.UI;
using Newtonsoft.Json;

#endregion

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