
$(document).ready(function () {
    $(window).resize(resizeEditor);
});

function resizeEditor() {
    var html = document.documentElement;
    var height = html.clientHeight;
    var width = html.clientWidth;
    if (CKEDITOR != 'undefined' && CKEDITOR.instances != 'undefined' && CKEDITOR.instances.editor1 != 'undefined') {
        CKEDITOR.instances.editor1.resize(isNaN(width) ? '300' : width, isNaN(height) ? '300' : height);
    }
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

function pasteHtmlAtCurrentPosition(html) {
    CKEDITOR.instances.editor1.insertHtml(html);
}

function pastePlaneAtCurrentPosition(planeText) {
    CKEDITOR.instances.editor1.insertText(planeText);
}

function start() {
    CKEDITOR.replace('editor1', {
        on:
        {
            //extraAllowedContent: 'img [ !src, alt, height, width, class, id ]',
            instanceReady: function (ev) {
                //editor.filter.check('img');
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
            items: ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', 'NumberedList', 'BulletedList', '-', 'Image', 'Link', 'Table', 'Source']
        }]
    });
};