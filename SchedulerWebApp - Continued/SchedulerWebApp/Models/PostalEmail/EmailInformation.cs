using System;

namespace SchedulerWebApp.Models.PostalEmail
{
    public class EmailInformation
    {
        public Event CurrentEvent { get; set; }
        public int ParticipantId { get; set; }
        public string OrganizerName { get; set; }
        public string OrganizerEmail { get; set; }
        public string ParticipantEmail { get; set; }
        public string EmailSubject { get; set; }
        public string ResponseUrl { get; set; }
        public string EventDetailsUrl { get; set; }
        public DateTime RemainderDate { get; set; }
        public DateTime ListDate { get; set; }
    }
}