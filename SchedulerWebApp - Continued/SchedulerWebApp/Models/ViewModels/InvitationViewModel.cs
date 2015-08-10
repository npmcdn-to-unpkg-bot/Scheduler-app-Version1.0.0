using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SchedulerWebApp.Models.Validation;

namespace SchedulerWebApp.Models.ViewModels
{
    public class InvitationViewModel
    {
        public int EventId { get; set; }

        [Display(Name = "Title")]
        public string EventTitle { get; set; }

        [Display(Name = "Location")]
        public string EventLocation { get; set; }

        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; }

        [Required]
        [ValidEmail]
        [Display(Name = "Email(s)")]
        [DataType(DataType.MultilineText)]
        [Remote("CheckParticipantEmail", "Invitation", ErrorMessage = "This participant is already invited.",
            AdditionalFields = "EventId")]
        public string ParticipantsEmails { get; set; }
    }
}