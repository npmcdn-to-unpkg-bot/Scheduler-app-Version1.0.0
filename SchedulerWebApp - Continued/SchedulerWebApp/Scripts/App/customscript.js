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

function linkTwoInputs(listDateElementId, remeinderElementId, maxDatetime) {

    var listId = '#' + listDateElementId;
    var remeinderId = '#' + remeinderElementId;
    var dateToday = todaysDate.subtract(0, 'days').startOf('day');

    $(listId).datetimepicker({
        format: "DD.MM.YYYY HH:mm",
        minDate: dateToday,
        maxDate: maxDatetime
    });

    $(remeinderId).datetimepicker({
        format: "DD.MM.YYYY HH:mm",
        minDate: dateToday,
        maxDate: maxDatetime,
        useCurrent: false //Important! See issue #1075
    });

    $(listId).on("dp.change", function (e) {
        $(remeinderId).data("DateTimePicker").maxDate(e.date);
        $(remeinderId).data("DateTimePicker").minDate(dateToday);
    });

    $(remeinderId).on("dp.change", function (e) {
        $(listId).data("DateTimePicker").minDate(e.date);
    });
}

function linkThreeDatesElements(eventDateElementId, listDateElementId, remainderDateElementId) {

    var eventDate = '#' + eventDateElementId, listDate = '#' + listDateElementId, remainderDate = '#' + remainderDateElementId;


    $(eventDate).datetimepicker({
        format: "DD.MM.YYYY HH:mm",
        minDate: todaysDate.subtract(0, 'days').startOf('day')
    });

    $(listDate).datetimepicker({
        format: "DD.MM.YYYY HH:mm",
        minDate: todaysDate.subtract(0, 'days').startOf('day'),
        useCurrent: false
    });

    $(remainderDate).datetimepicker({
        format: "DD.MM.YYYY HH:mm",
        minDate: todaysDate.subtract(0, 'days').startOf('day'),
        useCurrent: false
    });

    $(eventDate).on("dp.change", function(e) {
        $(listDate).data("DateTimePicker").maxDate(e.date);
        $(remainderDate).data("DateTimePicker").maxDate(e.date);
    });

    $(listDate).on("dp.change", function (e) {
        $(remainderDate).data("DateTimePicker").maxDate(e.date);
    });
}

function convertInputDateToMomentDate(inputElementId) {
    var id = '#' + inputElementId;

    var startDate = $(id).val();
    console.log(startDate);

    var datetimeArray = startDate.split(' ');

    var dateArray = datetimeArray[0].split('\.');
    var timeArray = datetimeArray[1].split(':');

    var dateTimeArray = $.merge(dateArray, timeArray);

    console.log(dateTimeArray);

    var eDate, eMonth, eYear, eHour, eMinutes;

    eDate = dateTimeArray[0];
    eMonth = dateTimeArray[1];
    eYear = dateTimeArray[2];
    eHour = dateTimeArray[3];
    eMinutes = dateTimeArray[4];
    var dateString = eYear + '-' + eMonth + '-' + eDate + ' ' + eHour + ':' + eMinutes;
    console.log(dateString);

    var maxDate = moment(dateString).toDate();
    console.log(maxDate);

    return maxDate;
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