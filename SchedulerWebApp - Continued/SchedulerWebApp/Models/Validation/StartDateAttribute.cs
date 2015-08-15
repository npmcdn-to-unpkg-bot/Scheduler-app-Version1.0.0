using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace SchedulerWebApp.Models.Validation
{
    public class StartDateAttribute : ValidationAttribute
    {
        public StartDateAttribute(string compareDate)
        {
            CompareDate = compareDate;
        }

        public string CompareDate { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            PropertyInfo propertyInfo = validationContext.ObjectType.GetProperty(CompareDate);
            object dateToCompare = propertyInfo.GetValue(validationContext.ObjectInstance);

            DateTime startDate = dateToCompare is DateTime ? (DateTime) dateToCompare : new DateTime();

            string dateString;
            DateTime dateTime;
            const string format = "d";

            if (value != null) dateString = String.Format("{0:d}", value);
            else dateString = string.Empty;

            /*var validDateformat = DateTime.TryParse(dateString, out dateTime);*/

            var validDateformat = DateTime.TryParseExact(dateString,
                                                          format,
                                                          new CultureInfo("de-CH"), 
                                                          DateTimeStyles.None, out dateTime);


            if (!validDateformat)
            {
                return new ValidationResult("Invalid date format");
            }
            var enteredDate = dateTime.Date;

            if (enteredDate.Date >= startDate.Date) return ValidationResult.Success;
            return new ValidationResult(ErrorMessage);
        }
    }
}