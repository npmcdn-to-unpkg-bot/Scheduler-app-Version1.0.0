//Hide remeinder input if checkbox is not checked
var isChecked = $('#remainderCheckbox:checked').length;

if (isChecked < 1) {
    $('#datePickerRemainder').val('');
    $('#remainderInput').hide();
}

$(function () {

    initTagIt();

    checkSupportForInputTypeDate();

    var maxDate = convertInputDateToMomentDate("HiddenDatePickerList");

    linkTwoInputs('datePickerList', 'datePickerRemainder', maxDate);
});

//Change button text on form submition or invitation cancellation.
$("form").submit(function (e) {
    var isValid = $('form').valid();

    //delay for 100 ms to let error s to be displayed
    setTimeout(function () {
        var numberOfErros = $('.field-validation-error').length;

        if (isValid && numberOfErros <= 0) {
            changeButtonText('submitButton', 'Sending');
        }
    }, 100);
});

$('#invitationCancelButton').on('click', function () {
    changeButtonText('invitationCancelButton', 'Cancelling');
});

//Hide or show remeinder field depending what user selected
$('#remainderCheckbox').change(function () {
    var remeinderInput = $('#remainderInput');
    var cheked = $('#remainderCheckbox:checked').length;

    if (cheked < 1) {
        console.log(cheked + ' > 1');
        console.log(' unchacked');
        remeinderInput.slideUp('slow');
    } else {
        console.log(cheked + ' = 1');
        console.log('Checked');
        $('#datePickerRemainder').val('');
        remeinderInput.slideDown('slow');
    }
});

function inputInit(elementId) {

    var todaysDate = moment(new Date());
    var element = '#' + elementId;

    $(element).datetimepicker({
        format: "DD.MM.YYYY HH:mm",
        minDate: todaysDate.subtract(0, 'days').startOf('day')
    });
}

function initTagIt() {

    $("#ParticipantsEmails").tagit({
        placeholderText: "Participant Emails separated by comma",
        afterTagAdded: adjustHeight,
        autocomplete: ({ source: dataSource })
    });
}

var dataSource = function (request, response) {
    $.ajax({
        url: '/Contacts/SearchContact',
        dataType: "json",
        data: { term: request.term },
        success: function (data) { response(data); }
    });
};

var adjustHeight = function (event, ui) {
    var enteredEmails = $('ul.tagit li').length;

    if (enteredEmails > 3) {
        $('.body-div').css("height", "100%");
    };
};

setDatesInCorrectFormat();

AllowValidationOnHiddenInputs();