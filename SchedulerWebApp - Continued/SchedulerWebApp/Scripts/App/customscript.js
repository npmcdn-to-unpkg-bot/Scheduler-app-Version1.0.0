/*
 * This is a Custom script which is used 
 * throughout the views
 */

var todaysDate = moment(new Date());

function elementsInitialization(elementId) {
    var id = '#' + elementId;
    $(id).datetimepicker({
        format: "DD.MM.YYYY HH:mm",
        minDate: todaysDate.subtract(0, 'days').startOf('day')
    });
}

function linkThreeDatesElements(maxDateElementId, dateElementId) {

    var maxDate = '#' + maxDateElementId, date = '#' + dateElementId;

    $(date).datetimepicker({
        format: "DD.MM.YYYY HH:mm"
    });

    $(maxDate).on("dp.change", function (e) {
        $(date).data("DateTimePicker").minDate(todaysDate);
        $(date).data("DateTimePicker").maxDate(e.date);
    });
}

//DataTable functions
function makeTableResponsive(tableClass, columnIndexForOrdering) {
    var selector = '.' + tableClass;
    $(selector).DataTable({
        "order": [[columnIndexForOrdering, "desc"]],
        responsive: true,
        searching: false,
        "pagingType": "simple",
        "bLengthChange": false
    });
}


function grayOutAbsentees() {
    $('#participantTable').each(function () {
        $('tr:contains("Absent")').each(function () {
            $(this).css('color', 'rgb(220, 161, 161)');
        });
    });
}

function setDatesInCorrectFormat() {
    $.validator.addMethod('date', function (value, element) {
        var d = new Date();
        return this.optional(element) || !/Invalid|NaN/.test(new Date(d.toLocaleDateString(value)));
    });
}

function AllowValidationOnHiddenInputs() {
    $.validator.setDefaults({ ignore: null });
}