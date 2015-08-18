/// <reference path="./typings/jquery/jquery.d.ts"/>
var Paranoia;
(function (Paranoia) {
    var Composition = (function () {
        function Composition() {
            this.editorId = "paranoia-editor";
        }
        Object.defineProperty(Composition.prototype, "editor", {
            get: function () {
                return this.quill;
            },
            enumerable: true,
            configurable: true
        });
        Composition.prototype.init = function (container) {
            this.container = container;

            var editor = $("div")
                .attr("id", this.editorId);

            this.container.append(editor);
            this.quill = new Quill("#" + this.editorId, {
                "styles": {
                    ".ql-editor": {
                        "background-color": "white",
                        "font-family": "Candara, Calibri, Segoe, 'Segoe UI', Optima, Arial, sans-serif",
                        "font-size": "16px"
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
        Composition.prototype.setHistory = function (history) {
            var h = $.parseHTML(history);
            throw new Error("Not yet implemented");
        };
        Composition.prototype.changeSignature = function (signature) {
            throw new Error("Not yet implemented");
        };
        return Composition;
    })();
    Paranoia.Composition = Composition;
})(Paranoia || (Paranoia = {}));
var Composition;
$(document).ready(function () {
    var container = $("#container");
    Composition = new Paranoia.Composition();
    Composition.init(container);
});
