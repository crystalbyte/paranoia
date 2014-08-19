using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace CKEditorDotNet {
    [ComVisible(true)]
    public class EditorObjectForScripting {

        private readonly CKEditor _editor;
        public EditorObjectForScripting(CKEditor editor) {
            _editor = editor;
        }

        public event EventHandler EditorCommandExecuted;

        private void OnEditorCommandExecuted(object sender, EventArgs e) {
            var handler = EditorCommandExecuted;
            if (handler != null)
                EditorCommandExecuted(sender, e);
        }

        public void EditorButtonClicked(string command) {
            Debug.WriteLine(command);
            OnEditorCommandExecuted(this, new EventArgs());
        }

        internal string EditorContent;
        public void TextChanged(string text) {
            EditorContent = text;
            _editor.ContentHtml = text;
        }

        public void Test()
        {

        }
    }
}
