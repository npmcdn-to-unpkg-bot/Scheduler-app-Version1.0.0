﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SchedulerWebApp.Models.ValidationAttributes;

namespace SchedulerWebApp.Models
{
    public class Event
    {
        public Event(){}

        public int Id { get; set; }

        [Required]
        [Display(Name = "What")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Where")]
        public string Location { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "On")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        [ValidDate(ErrorMessage = "You cant create event in past dates / time")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Confirmations")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime? ListDate { get; set; }

        [Display(Name = "Remainders")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime? ReminderDate { get; set; }

        public virtual string SchedulerUserId { get; set; }
        public virtual List<Participant> Participants { get; set; }
    }
}