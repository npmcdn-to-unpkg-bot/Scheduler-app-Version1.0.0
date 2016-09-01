using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Elmah;

namespace SchedulerWebApp.Models.ValidationAttributes
{
    public class ValidDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string dateString;
            if (value != null)
            {
                dateString = String.Format("{0:g}", value);
            }
            else return new ValidationResult("Date is Required!");

            var dateTimeNowUtc = DateTime.UtcNow;
            var currentSwissTime = ConvertDateTime.ToSwissTimezone(dateTimeNowUtc);

            /*
             * Todo: the line below is only for debugging purposes:
             *      Its to log current time to see if the application is using client or server machine
             *      local time
             */
            ErrorSignal.FromCurrentContext().Raise(new Exception("The Current local machine time is: " + currentSwissTime.ToString("g") + " - { From Validation attribute }"));

            DateTime dateTime;
            const string format = "g";

            var validDateformat = DateTime.TryParseExact(dateString,
                                                         format,
                                                         new CultureInfo("de-CH"),
                                                         DateTimeStyles.None, out dateTime);

            if (!validDateformat)
            {
                return new ValidationResult("Invalid date format");
            }

            var enteredDateUtc = TimeZoneInfo.ConvertTimeToUtc(dateTime);
            var enteredDate = ConvertDateTime.ToSwissTimezone(enteredDateUtc);
            ErrorSignal.FromCurrentContext().Raise(new Exception("Entered date time is: " + enteredDate.ToString("g") + " - { From Validation attribute }"));

            if (enteredDate >= currentSwissTime)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage);
        }


    }
}