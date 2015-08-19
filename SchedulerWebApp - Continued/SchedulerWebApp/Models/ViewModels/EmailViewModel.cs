﻿using System;

namespace SchedulerWebApp.Models.ViewModels
{
    public class EmailViewModel
    {
        public int EventsId { get; set; }
        public string EventTitle { get; set; }
        public string EventLocation { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime GetListDate { get; set; }
        public int ParticipantId { get; set; }

        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string ReceiverEmail { get; set; }
        public string EmailSubject { get; set; }

        public string OrganizerName { get; set; }
        public string OrganizerEmail { get; set; }
    }
}