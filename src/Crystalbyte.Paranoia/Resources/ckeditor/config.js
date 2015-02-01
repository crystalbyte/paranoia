/**
 * @license Copyright (c) 2003-2014, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.md or http://ckeditor.com/license
 */

CKEDITOR.editorConfig = function (config) {
    // Define changes to default configuration here.
    // For complete reference see:
    // http://docs.ckeditor.com/#!/api/CKEDITOR.config

    config.toolbar = [
        { name: 'undo', items: ['Undo', 'Redo'] },
        { name: 'colors', items: ['TextColor', 'BGColor'] },
        { name: 'basicstyles', groups: ['basicstyles', 'cleanup'], items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'] },
        { name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align', 'bidi'], items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote', 'CreateDiv', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', '-', 'BidiLtr', 'BidiRtl', 'Language'] }
    ];

    config.allowedContent = true;
    config.extraAllowedContent = 'div[id];p[id];hr;img[!src,alt,width,height];a[!href];table;th;tr;td;*{*}';

    config.removePlugins = 'magicline,elementspath,contextmenu';

    // Set the most common block elements.
    config.format_tags = 'p;h1;h2;h3;pre;div';
};
