/// <reference path="./typings/jquery/jquery.d.ts"/>
var Paranoia;
(function (Paranoia) {
    var Composition = (function () {
        function Composition() {
        }
        Object.defineProperty(Composition.prototype, "editor", {
            get: function () {
                return this.quill;
            },
            enumerable: true,
            configurable: true
        });
        Composition.prototype.init = function (container) {
            this.quill = new Quill(container, {
                "styles": {
                    ".ql-editor": {
                        "background-color": "white"
                    }
                }
            });
            this.quill.on('text-change', function (delta, source) {
                // Extern is defined outside this document ...
                var json = JSON.stringify(delta);
                Extern.notifyTextChanged(source, json);
            });
            this.quill.on('selection-change', function (range) {
                // Extern is defined outside this document ...
                var json = JSON.stringify(range);
                Extern.notifySelectionChanged(json);
            });
            Extern.notifyContentReady();
        };
        Composition.prototype.changeSignature = function (encoded) {
            var signature = decodeURIComponent(encoded);

            var html = $(this.editor.getHTML());
            html.remove(".ql-signature");

            var div = $("<div>")
                .attr("id", "ql-signature")
                .html(signature);

            html.append(div)
            this.editor.setHTML(html.prop("innerHTML"));
        };
        return Composition;
    })();
    Paranoia.Composition = Composition;
})(Paranoia || (Paranoia = {}));
var Composition;
$(document).ready(function () {
    Composition = new Paranoia.Composition();
    Composition.init("#container");
});
