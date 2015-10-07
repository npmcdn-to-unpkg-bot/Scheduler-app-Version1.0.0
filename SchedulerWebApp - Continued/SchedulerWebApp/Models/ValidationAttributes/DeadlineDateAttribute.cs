using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace SchedulerWebApp.Models.ValidationAttributes
{
    public class DeadlineDateAttribute : ValidationAttribute
    {
        public DeadlineDateAttribute(string compareDate)
        {
            CompareDate = compareDate;
        }

        public string CompareDate { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            PropertyInfo propertyInfo = validationContext.ObjectType.GetProperty(CompareDate);
            object dateToCompare = propertyInfo.GetValue(validationContext.ObjectInstance);

            DateTime comparisonDate = dateToCompare is DateTime ? (DateTime)dateToCompare : new DateTime();

            string dateString;
            DateTime dateTime;
            const string format = "d/M/yyyy";


            dateString = value != null ? String.Format("{0:d/M/yyyy}", value) : string.Empty;

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

            if (enteredDate.Date <= comparisonDate.Date)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage);
        }
    }
}