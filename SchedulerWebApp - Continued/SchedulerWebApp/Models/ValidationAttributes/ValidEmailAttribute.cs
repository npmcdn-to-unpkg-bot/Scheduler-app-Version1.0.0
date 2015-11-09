using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;

namespace SchedulerWebApp.Models.ValidationAttributes
{
    public class ValidEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string enteredEmails = value != null ? value.ToString() : string.Empty;

            //split entered values
            string[] enteredValues = enteredEmails.Split(',');
            var emails = new List<string>();

            foreach (string e in enteredValues)
            {
                string enteredEmail = Service.RemoveBrackets(e);
                emails.Add(enteredEmail);
            }

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