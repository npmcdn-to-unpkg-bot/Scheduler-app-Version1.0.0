$(function () {

    elementsInitialization("datePickerStart");

    $('#cancelButton').on('click', function () {
        changeButtonText('cancelButton', 'Cancelling');
    });

    onValidFormSubmit("savebtn", "Saving");

    setDatesInCorrectFormat();

    checkSupportForInputTypeDate();

});