$(function () {
    makeTableResponsive("detailsTable", 1, 2);

    grayOutAbsentees();

    toolTipInit();

    displayAlert("Invitations sent.");
});

$('#participantTable').on('page.dt', function () {

    $(this).on('draw.dt', function () {
        grayOutAbsentees();
        toolTipInit();
    });

});

$(window).bind("load", function () {
    var rowCount = $('#participantTable tbody tr').length;

    if (rowCount >= 5) {
        console.log(rowCount);
        $('.body-div').css({
            'height': '100%'
        });
    }
});