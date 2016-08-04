$(function () {
    linkThreeDatesElements('datePickerStart', 'datePickerList', 'datePickerRemainder');
});

$('#cancelButton').on('click', function () {
    changeButtonText('cancelButton', 'Cancelling');
});

onValidFormSubmit("saveButton", "Saving");

$.validator.addMethod('date', function (value, element) {
    var d = new Date();
    return this.optional(element) || !/Invalid|NaN/.test(new Date(d.toLocaleDateString(value)));
});