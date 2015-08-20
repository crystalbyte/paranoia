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
                        //"font-family": "Candara, Calibri, Segoe, 'Segoe UI', Optima, Arial, sans-serif",
                        //"font-size": "16px"
                    }
                }
            });
            this.quill.addContainer("ql-signature");
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
            var signature = $.base64.decode(encoded);
            var sign = $(".ql-signature");
            sign.html(signature);
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
