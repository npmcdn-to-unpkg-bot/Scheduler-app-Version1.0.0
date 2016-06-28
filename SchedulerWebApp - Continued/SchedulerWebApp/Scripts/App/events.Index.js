//#region Masonry(arrange event divs)
var container = $('.masonry-container');

$(window).bind("load", function () {
    addMasonry(container);
});

$(document).ready(function () {

    $('a[data-toggle=tab]').on('click', adjustHeight);

    createMap(".caption", ".location");

    $('a[data-toggle=tab]').each(function () {
        var self = $(this);
        self.on('shown.bs.tab', function () {
            addMasonry(container);
            fixContainerHeight();
        });
    });


});
//#endregion

function addMasonry(masonryContainer) {
    masonryContainer.masonry({
        columnWidth: '.masonry-item',
        itemSelector: '.masonry-item'
    });
}

toolTipInit();

(function () {

    trimOnSmallScreens();

    function trimText(elementSelector, maxCharacter) {
        $(elementSelector).each(function () {
            var header = $.trim($(this).text());

            if (header.length > maxCharacter) {
                $(this).text(header.substr(0, maxCharacter) + "...");
            }
        });
    }

    function trimOnSmallScreens() {
        var width = $(window).width();
        if (width < 768) {
            trimText(".p-header", 10);
        } else {
            trimText(".p-header", 15);
        }
    }
})();
