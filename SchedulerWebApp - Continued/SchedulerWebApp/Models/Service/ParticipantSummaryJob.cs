using System.Data.Entity;
using System.Linq;
using Quartz;
using SchedulerWebApp.Controllers;
using SchedulerWebApp.Models.DBContext;
using SchedulerWebApp.Models.ViewModels;

namespace SchedulerWebApp.Models.Service
{
    public class ParticipantSummaryJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            var db = new SchedulerDbContext();

            JobDataMap dataMap = context.JobDetail.JobDataMap;

            //get event id, receiver email and receiver name from datamap instance
            var eventId = dataMap.GetIntValue("eventId");
            var organizerName = dataMap.GetString("organizerName");
            var userEmail = dataMap.GetString("userEmail");

            //Get event 
            var @event = db.Events.Where(e => e.Id == eventId)
                                  .Include(e => e.Participants)
                                  .FirstOrDefault();

            if (@event == null) return;

            var participants = @event.Participants.ToList();

            var allParticipant = participants.Count();
            var respondedParticipants = participants.Count(p => p.Responce);
            var notResponded = allParticipant - respondedParticipants;
            var attendingParticipant = participants.Count(p => p.Availability);
            var notAttendingParticipant = allParticipant - attendingParticipant;


            //create email 
            var email = new ParticipantListViewModel
                        {
                            EventsId = @event.Id,   
                            EventsTitle = @event.Title,
                            ReceiverEmail = userEmail,
                            OrganizerName = organizerName,
                            AllParticipant = allParticipant,
                            ParticipantAttending = attendingParticipant,
                            ParticipantNotAttending = notAttendingParticipant,
                            ParticipantNotResponded = notResponded,
                            ParticipantResponded = respondedParticipants,
                            EmailSubject = "Participants summary" + " " + @event.Title
                        };

            //send email
            var emailDelivery = new EmailDeliveryController();
            emailDelivery.DeliverSummary(email, "ParticipantSummary").Deliver();
            
        }
    }
}