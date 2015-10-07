using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;

namespace SchedulerWebApp.Models.ValidationAttributes
{
    public class ValidEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string enteredEmails;
            if (value != null) enteredEmails = value.ToString();
            else enteredEmails = string.Empty;

            //split emails
            string[] emails = enteredEmails.Split(',');
            if (emails.Any(email => !IsValid(email)))
            {
                return new ValidationResult("Email(s) Not correct format.\n Enter email separated by comma");
            }
            return ValidationResult.Success;
        }

        //validate email format
        public bool IsValid(string emailaddress)
        {
            try
            {
                if (string.IsNullOrEmpty(emailaddress))
                {
                    return false;
                }
                new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

    }
}