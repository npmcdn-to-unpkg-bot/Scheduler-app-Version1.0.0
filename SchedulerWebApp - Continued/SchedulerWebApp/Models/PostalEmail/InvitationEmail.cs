using System;
using Postal;

namespace SchedulerWebApp.Models.PostalEmail
{
    public class InvitationEmail : Email
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
        public string ResponseUrl { get; set; }
        public string EventDetailsUrl { get; set; }


        public int AllParticipants { get; set; }
        public int ParticipantAttending { get; set; }
        public int ParticipantNotAttending { get; set; }
        public int ParticipantsResponded { get; set; }
        public int ParticipantsNotResponded { get; set; }
    }
}