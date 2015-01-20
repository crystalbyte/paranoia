$('a').click(function (ev) {
    var clicked = $(this).attr('href');
    external.onLinkClicked(clicked);
    ev.preventDefault();
});