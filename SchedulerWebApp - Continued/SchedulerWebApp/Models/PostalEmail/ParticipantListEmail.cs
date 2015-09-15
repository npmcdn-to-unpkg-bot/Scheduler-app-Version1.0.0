namespace SchedulerWebApp.Models.PostalEmail
{
    public class ParticipantListEmail : InvitationEmail
    {
        public int AllParticipants { get; set; }
        public int ParticipantAttending { get; set; }
        public int ParticipantNotAttending { get; set; }
        public int ParticipantsResponded { get; set; }
        public int ParticipantsNotResponded { get; set; }
    }
}