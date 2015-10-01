/*
 * This is a Custom script which is used 
 * throughout the views
 */

var todaysDate = moment(new Date());

function elementsInitialization() {
    $('#datePickerStart').datetimepicker({
        format: "DD.MM.YYYY",
        minDate: todaysDate.subtract(0, 'days').startOf('day')
    });

    $('#datePickerList').datetimepicker({
        format: "DD.MM.YYYY"
    });
}

function linkStartAndListDates() {
    $('#datePickerStart').on("dp.change", function (e) {
        $('#datePickerList').data("DateTimePicker").minDate(todaysDate);
        $('#datePickerList').data("DateTimePicker").maxDate(e.date);
    });

    $('#datePickerList').on("dp.change", function (e) {
        var originalMaxDate = e.date;
        var newMax = new Date(originalMaxDate);
        newMax.setDate(newMax.getDate() + 365);
        var startMaxDate = newMax;

        $('#datePickerStart').data("DateTimePicker").maxDate(startMaxDate);
    });
}

function linkThreeDatesElements(maxDateElementId, dateElementId) {

    var maxDate = '#' + maxDateElementId, date = '#' + dateElementId;

    $(date).datetimepicker({
        format: "DD.MM.YYYY"
    });

    $(maxDate).on("dp.change", function (e) {
        $(date).data("DateTimePicker").minDate(todaysDate);
        $(date).data("DateTimePicker").maxDate(e.date);
    });
}

//DataTable functions
function makeTableResponsive(tableClass) {
    var selector = '.' + tableClass;
    $(selector).DataTable({
        responsive: true,
        searching: false,
        "pagingType": "simple"
    });
}