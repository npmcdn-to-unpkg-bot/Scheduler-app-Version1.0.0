namespace SchedulerWebApp.Models.ViewModels
{
    public class ParticipantListViewModel
    {
        public int EventsId { get; set; }
        public string EventsTitle { get; set; }
        public string OrganizerName { get; set; }
        public string ReceiverEmail { get; set; }
        public string EmailSubject { get; set; }
        public int AllParticipant { get; set; }
        public int ParticipantResponded { get; set; }
        public int ParticipantNotResponded { get; set; }
        public int ParticipantAttending { get; set; }
        public int ParticipantNotAttending { get; set; }
    }
}