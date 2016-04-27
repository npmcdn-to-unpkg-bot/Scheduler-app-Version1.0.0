using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SchedulerWebApp.Models.ValidationAttributes
{
    public class CustomRequiredRemainderDateAttribute : ValidEmailAttribute
    {
        public CustomRequiredRemainderDateAttribute(string remainderCondition)
        {
            RemainderCondition = remainderCondition;
        }
        public string RemainderCondition { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            PropertyInfo propertyInfo = validationContext.ObjectType.GetProperty(RemainderCondition);
            object remainderRequiredString = propertyInfo.GetValue(validationContext.ObjectInstance);

            var isRemainderRequired = Convert.ToBoolean(remainderRequiredString);
            if (isRemainderRequired)
            {
                return value != null ? ValidationResult.Success : new ValidationResult("Remainder date is Required");
            }
            return ValidationResult.Success;
        }
    }
}