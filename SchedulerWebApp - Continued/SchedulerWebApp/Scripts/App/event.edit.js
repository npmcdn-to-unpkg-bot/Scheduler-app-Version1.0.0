$(function () {
    linkThreeDatesElements('datePickerStart', 'datePickerList', 'datePickerRemainder');
});

$.validator.addMethod('date', function (value, element) {
    var d = new Date();
    return this.optional(element) || !/Invalid|NaN/.test(new Date(d.toLocaleDateString(value)));
});