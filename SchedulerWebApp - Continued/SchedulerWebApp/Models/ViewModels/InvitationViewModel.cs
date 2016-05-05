using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SchedulerWebApp.Models.ValidationAttributes;

namespace SchedulerWebApp.Models.ViewModels
{
    public class InvitationViewModel
    {
        public int EventId { get; set; }

        [Display(Name = "What")]
        public string EventTitle { get; set; }

        [Display(Name = "Where")]
        public string EventLocation { get; set; }

        [Display(Name = "When")]
        [DataType(DataType.Text)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}",ApplyFormatInEditMode = true)]
        public DateTime EventDate { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Confirmations")]
        [ValidDate(ErrorMessage = "Get list between today and end date")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [DeadlineDate("EventDate", ErrorMessage = "Date between today and event's date")]
        public DateTime? ListDate { get; set; }

        //[Required]
        [DataType(DataType.Text)]
        [Display(Name = "Reminder(s)")]
        //[ValidDate(ErrorMessage = "Remind between today and list date")]
        //[DeadlineDate("ListDate", ErrorMessage = "Remind between today and list date")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [CustomRequiredRemainderDate("SendRemainder")]
        public DateTime? ReminderDate { get; set; }

        [Required]
        [InvitedOnce("EventId")]
        [ValidEmail]
        [Display(Name = "Email(s)")]
        [Remote("CheckParticipantEmail", "Invitation", 
                 ErrorMessage = "This participant is already invited.",
                 AdditionalFields = "EventId")]
        public string ParticipantsEmails { get; set; }

        [Display(Name = "Send Remainder(s)")]
        public bool SendRemainder { get; set; }
    }
}