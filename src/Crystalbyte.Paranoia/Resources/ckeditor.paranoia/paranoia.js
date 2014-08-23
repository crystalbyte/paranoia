﻿function resizeEditor() {
    var html = document.documentElement;
    var height = html.clientHeight;
    if (CKEDITOR != 'undefined' && CKEDITOR.instances != 'undefined' && CKEDITOR.instances.editor1 != 'undefined')
        CKEDITOR.instances.editor1.resize('100%', isNaN(height) ? '300' : height);
}
function getEditorHtml() {
    return CKEDITOR.instances.editor1.getData();
}

function setEditorHtml(html) {
    CKEDITOR.instances.editor1.setData(html);
}

function handleAfterCommandExec(event) {
    var commandName = event.data.name;
    window.external.EditorButtonClicked(commandName);
}

function execCommand(command) {
    CKEDITOR.instances.editor1.execCommand(command);
}

function setEditorReadonlyState(readOnly) {
    CKEDITOR.instances.editor1.setReadOnly(readOnly);
}

function Stuff() {
    window.external.Test(CKEDITOR.instances.editor1.getSelection());
}

function start() {
    CKEDITOR.replace('editor1', {
        on:
        {
            instanceReady: function (ev) {
                resizeEditor();
            }
            //},
            //afterCommandExec: function (ev) {
            //    handleAfterCommandExec(ev);
            //},
            //change: function (ev) {
            //    window.external.TextChanged(CKEDITOR.instances.editor1.getData());
            //}

        },
        toolbar: [
        {
            name: 'document',
            items: ['Undo', 'Redo', 'Font', 'FontSize']
        },
        {
            name: 'document1',
            items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', 'TextColor', 'BGColor']
        },
        {
            name: 'document2',
            groups: ['justify', 'mode'],
            items: ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', 'NumberedList', 'BulletedList', '-', 'Link', 'Table', 'Source']
        }]
    });
};