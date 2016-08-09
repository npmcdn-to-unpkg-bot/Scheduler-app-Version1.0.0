using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using SchedulerWebApp.Models.DBContext;

namespace SchedulerWebApp.Models.ValidationAttributes
{
    public class InvitedOnceAttribute : ValidationAttribute
    {
        private readonly Service _service = new Service();
        public InvitedOnceAttribute(string eventId)
        {
            EventId = eventId;
        }
        public string EventId { get; set; }


        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string enteredEmails;
            if (value != null)
            {
                enteredEmails = value.ToString();
            }
            else
            {
                enteredEmails = string.Empty;
            }
            List<string> emails = enteredEmails.Split(',').ToList();

            PropertyInfo propertyInfo = validationContext.ObjectType.GetProperty(EventId);
            object currentEventId = propertyInfo.GetValue(validationContext.ObjectInstance);

            int id;
            if (currentEventId is int)
            {
                id = (int)currentEventId;
            }
            else
            {
                id = 0;
            }

            using (var dbContext = new SchedulerDbContext())
            {
                Event currentEvent = dbContext.Events.Find(id);
                foreach (var e in emails)
                {
                    var email = _service.RemoveBrackets(e);

                    bool isNotYetInvited = currentEvent.Participants.All(p => p.Email != email);

                    if (!isNotYetInvited)
                    {
                        var errorMsg = String.Format(@"{0} is already invited", email);
                        return new ValidationResult(errorMsg);
                    }
                }
            }
            return ValidationResult.Success;
        }
    }
}