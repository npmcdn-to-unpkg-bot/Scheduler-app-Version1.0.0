using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;

namespace SchedulerWebApp.Models.ValidationAttributes
{
    public class ValidEmailAttribute : ValidationAttribute
    {
        private readonly Service _service;

        public ValidEmailAttribute()
        {
            _service = new Service();
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string enteredEmails = value != null ? value.ToString() : string.Empty;

            //split entered values
            string[] enteredValues = enteredEmails.Split(',');
            var emails = new List<string>();

            foreach (var e in enteredValues)
            {
                var enteredEmail = _service.RemoveBrackets(e);
                emails.Add(enteredEmail);
            }

            foreach (var email in emails.Where(email => !IsValid(email)))
            {
                return new ValidationResult(String.Format("{0} is not in a correct format.", email));
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