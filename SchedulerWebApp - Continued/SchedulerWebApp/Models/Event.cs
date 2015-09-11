using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SchedulerWebApp.Models.Validation;

namespace SchedulerWebApp.Models
{
    public class Event
    {
        public Event()
        {
        }
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Location { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Start")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [ValidDate(ErrorMessage = "You cant create event in past dates")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End")]
        [DataType(DataType.Text)]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [StartDate("StartDate", ErrorMessage = "Events ends before event's start date")]
        public DateTime EndDate { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Remind On")]
        [ValidDate(ErrorMessage = "Remind between today and list date")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [DeadlineDate("ListDate", ErrorMessage = "Remind between today and list date")]
        public DateTime ReminderDate { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Get list On")]
        [ValidDate(ErrorMessage = "Get list between today and end date")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [DeadlineDate("EndDate", ErrorMessage = "Get list between today and end date")]
        public DateTime ListDate { get; set; }

        public virtual string SchedulerUserId { get; set; }
        public virtual List<Participant> Participants { get; set; }
    }
}