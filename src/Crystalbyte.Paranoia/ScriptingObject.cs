using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.UI;

namespace Crystalbyte.Paranoia {
    public sealed class ScriptingObject {
        private readonly HtmlEditor _editor;

        public ScriptingObject(HtmlEditor editor) {
            _editor = editor;
        }
    }
}
