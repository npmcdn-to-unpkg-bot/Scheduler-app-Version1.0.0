namespace SchedulerWebApp.Models.PostalEmail
{
    public class EmailInformation
    {
        public Event CurrentEvent { get; set; }
        public int ParticipantId { get; set; }
        public string OrganizerName { get; set; }
        public string OrganizerEmail { get; set; }
        public string ParticipantEmail { get; set; }
        public string ResponseUrlString { get; set; }
    }
}