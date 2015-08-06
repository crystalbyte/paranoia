/// <reference path="./typings/jquery/jquery.d.ts"/>

// JQuery typing surrogate.
declare var $; 
declare var JQuery;

// The quill editor has no defined typings.
declare var Quill;

// The extern object represents the CEF scripting object.
declare var Extern;

module Paranoia {
    export class Composition {
        private editorId = "paranoia-editor";
        private container: JQuery;
        private quill: any;

        get editor(): any {
            return this.quill;
        }

        public init(container: JQuery) {
            this.container = container;

            var editor = $("div")	
                .attr("id", this.editorId)
                .addClass("mine");

            this.container.append(editor);

            this.quill = new Quill("#" + this.editorId, {
                'styles': {
                    '.ql-editor': {
                        'font-family': "Candara, Calibri, Segoe, 'Segoe UI', Optima, Arial, sans-serif",
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
        }

        public setHistory(history: string): void {
            var h = $.parseHTML(history);
            throw new Error("Not yet implemented");
        }

        public changeSignature(signature: string): void {
            throw new Error("Not yet implemented");
        }
    }
}

$(document).ready(function () {
    var container = $("#container");
	var composition = new Paranoia.Composition();
    composition.init(container);

	$(this).Composition = composition;
});