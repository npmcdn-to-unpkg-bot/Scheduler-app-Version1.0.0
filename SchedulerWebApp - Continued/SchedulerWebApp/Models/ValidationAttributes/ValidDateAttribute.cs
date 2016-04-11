using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace SchedulerWebApp.Models.ValidationAttributes
{
    public class ValidDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string dateString;
            if (value != null) dateString = String.Format("{0:g}", value);
            else return new ValidationResult("Date is Required!");

            DateTime dateToday = DateTime.UtcNow.ToLocalTime();
            DateTime dateTime;
            const string format = "g";

            /*var validDateformat = DateTime.TryParse(dateString, out dateTime);*/

            var validDateformat = DateTime.TryParseExact(dateString,
                                                         format,
                                                         new CultureInfo("de-CH"), 
                                                         DateTimeStyles.None, out dateTime);

            if (!validDateformat)
            {
                return new ValidationResult("Invalid date format");
            }
            var enteredDate = dateTime;

            if (enteredDate >= dateToday)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage);
        }
    }
}