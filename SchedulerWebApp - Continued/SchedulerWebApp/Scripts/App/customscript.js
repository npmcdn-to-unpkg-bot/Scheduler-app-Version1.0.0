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

function linkTwoInputs(firstInput, secondInput) {

    var firstInputId = '#' + firstInput;
    var secondInputId = '#' + secondInput;

    $(firstInputId).datetimepicker({
        format: "DD.MM.YYYY HH:mm",
        minDate: todaysDate.subtract(0, 'days').startOf('day')
    });

    $(secondInputId).datetimepicker({
        format: "DD.MM.YYYY HH:mm",
        useCurrent: false //Important! See issue #1075
    });

    $(firstInputId).on("dp.change", function (e) {
        $(secondInputId).data("DateTimePicker").minDate(e.date);
    });
    $(secondInputId).on("dp.change", function (e) {
        $(firstInputId).data("DateTimePicker").maxDate(e.date);
    });
}

/*function linkThreeDatesElements(maxDateElementId, dateElementId) {

    var maxDate = '#' + maxDateElementId, date = '#' + dateElementId;

    $(date).datetimepicker({
        format: "DD.MM.YYYY HH:mm"
    });

    $(maxDate).on("dp.change", function (e) {
        $(date).data("DateTimePicker").minDate(todaysDate);
        $(date).data("DateTimePicker").maxDate(e.date);
    });
}*/

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