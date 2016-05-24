/*
 * This is a Custom script which is used 
 * throughout the application
 */

var todaysDate = moment(new Date());

function elementsInitialization(elementId) {
    var id = '#' + elementId;
    $(id).datetimepicker({
        format: "DD.MM.YYYY HH:mm",
        toolbarPlacement: 'top',
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
        toolbarPlacement: 'top',
        maxDate: maxDatetime
    });

    $(remeinderId).datetimepicker({
        format: "DD.MM.YYYY HH:mm",
        minDate: dateToday,
        toolbarPlacement: 'top',
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
        toolbarPlacement: 'top',
        minDate: todaysDate.subtract(0, 'days').startOf('day')
    });

    $(listDate).datetimepicker({
        format: "DD.MM.YYYY HH:mm",
        toolbarPlacement: 'top',
        minDate: todaysDate.subtract(0, 'days').startOf('day'),
        useCurrent: false
    });

    $(remainderDate).datetimepicker({
        format: "DD.MM.YYYY HH:mm",
        toolbarPlacement: 'top',
        minDate: todaysDate.subtract(0, 'days').startOf('day'),
        useCurrent: false
    });

    $(eventDate).on("dp.change", function (e) {
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
function makeTableResponsive(tableClass, firstColumnIndexForOrdering, secondColumnIndexForOrdering) {
    var selector = '.' + tableClass;
    $(selector).DataTable({
        "order": [[firstColumnIndexForOrdering, "asc"], [secondColumnIndexForOrdering, "asc"]],
        responsive: true,
        searching: false,
        "pagingType": "simple",
        "bLengthChange": false
    });
}

function grayOutAbsentees() {
    $('#participantTable').each(function () {
        $('tr:contains("Absent")').each(function () {
            $(this).css('color', 'pink');
            $(this).find('td .fa-calendar-times-o').addClass('element-red');
            $(this).find('td .fa-check-square-o').addClass('element-green');

        });
        $('tr:contains("Attending")').each(function () {
            $(this).addClass('element-green');
        });

        $('.colorSpan:contains("Absent No Response")').each(function () {
            $(this).closest('tr').css('color', 'lightgray');
        });
    });
}

function setDatesInCorrectFormat() {

    $.validator.addMethod('date', function (value, element) {
        var d = new Date();
        return this.optional(element) || !/Invalid|NaN/.test(new Date(d.toLocaleDateString(value)));
    });
}

function checkSupportForInputTypeDate() {
    jQuery.validator.methods.date = function (value, element) {
        var isChrome = /Chrome/.test(navigator.userAgent) && /Google Inc/.test(navigator.vendor);
        var isSafari = /Safari/.test(navigator.userAgent) && /Apple Computer/.test(navigator.vendor);
        if (isSafari || isChrome) {
            return true;
        } else {
            return this.optional(element) || !/Invalid|NaN/.test(new Date(value));
        }
    };
}

function AllowValidationOnHiddenInputs() {
    $.validator.setDefaults({ ignore: null });
}

function showHidePassword(iconSelector, inputIdSelector) {
    $(iconSelector).on('click', function () {

        var inputElement = $(inputIdSelector);
        var inputType = inputElement.attr('type');

        if (inputType == "password") {

            inputElement.attr('type', 'text');
            $(iconSelector + ' > .glyphicon-eye-open').removeClass('glyphicon-eye-open').addClass('glyphicon-eye-close');

        } else if (inputType == "text") {

            inputElement.attr('type', 'password');
            $(iconSelector + ' > .glyphicon-eye-close').removeClass('glyphicon-eye-close').addClass('glyphicon-eye-open');
        }
    });
}

function changeButtonText(buttonId, newText) {
    var id = '#' + buttonId;
    var newElement = '<span>' + newText + '.. <i class="fa fa-spinner fa-lg fa-spin fa-fw margin-bottom"></i><span class="sr-only">' + newText + ' in...</span></span>';
    $(id).html(newElement);
}

function fixContainerHeight() {
    var windowHeight = $(window).height();
    var containerHeight = $('.body-div').height();

    if (containerHeight < windowHeight) {

        $('.body-div').css("height", "85vh");
    }
}

function adjustHeight() {
    var events = $(".event-div").length;
    if (events > 6) {
        $(".body-div").css('height', '100%');
    }
}

/*not neededd anymore*/
function centrelizeDiv(minusTopPosition, checkWidth) {

    //get size of the browserName window
    var width = $(window).width();
    console.log(width);

    if (checkWidth == true) {
        if (width < 992) {
            console.log("Check width: " + checkWidth);
            $("div.centered-row").removeAttr('style');
            return;
        } else {
            setStyles();
        }
    }

    if (minusTopPosition == undefined) {
        return;
    } else {
        setStyles();
    }

    function setStyles() {
        minusTopPosition = minusTopPosition + "px";
        var styles = {
            position: "absolute",
            margin: "auto",
            top: minusTopPosition,
            right: 0,
            bottom: 0,
            left: 0,
            height: "100px"
        };
        console.log("Check width: " + checkWidth);
        $("div.centered-row").css(styles);
    }
}

function displayAlert(message) {
    console.log(Cookies.get('successCookie'));
    if (Cookies.get('successCookie') != undefined) {

        $('#alertMessage').text(message);
        $("#cookie-div").slideDown(800).delay(3000).slideUp(1000);
    }
    Cookies.remove('successCookie');
    console.log(Cookies.get('successCookie'));
}

//#region adding Map into evrnt div
function createMap(selector, locationElementSelctor) {
    var geocoder = new google.maps.Geocoder();

    $(selector).each(function () {
        var address = $(this).find(locationElementSelctor).text();
        console.log(address);

        geocoder.geocode({ 'address': address },
            function (results, status) {
                if (status === google.maps.GeocoderStatus.OK) {
                    address = address;
                } else {
                    console.log('Geocode for ' + address + ' was not successful for the following reason: ' + status);
                    address = "winterthur";
                }
            });

        $(this).find("img").attr('src', 'https://maps.googleapis.com/maps/api/staticmap?center=' + address + '&zoom=14&size=350x250&markers=color:red%7Clabel:0%7C11211%7C11206%7C11222|' + address + '&key=AIzaSyB7hmntE1W-a7pmy7UoocDrQlawzUujTwI');

    });
}

//#endregion 