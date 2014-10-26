$('a').click(function (ev) {
    var clicked = $(this).attr('href');
    external.OnLinkClicked(clicked);
    ev.preventDefault();
});