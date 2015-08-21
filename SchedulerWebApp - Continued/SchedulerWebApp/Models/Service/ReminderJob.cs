using System.Data.Entity;
using System.Linq;
using Quartz;
using SchedulerWebApp.Controllers;
using SchedulerWebApp.Models.DBContext;
using SchedulerWebApp.Models.PostalEmail;
using SchedulerWebApp.Models.ViewModels;

namespace SchedulerWebApp.Models.Service
{
    public class ReminderJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            var db = new SchedulerDbContext();

            JobDataMap dataMap = context.JobDetail.JobDataMap;

            //get event id,sender email and sendername from datamap instance
            var eventId = dataMap.GetIntValue("eventId");
            var senderName = dataMap.GetString("senderName");
            var senderEmail = dataMap.GetString("senderEmail");

            //Get event 
            var @event = db.Events.Where(e=>e.Id == eventId)
                                  .Include(e=> e.Participants)
                                  .FirstOrDefault();

            if (@event == null) return;

            var noResponseParticipants = @event.Participants
                                               .Where(p => p.Responce == false);

            foreach (var participant in noResponseParticipants)
            {
                //create email
                var email = new InvitationEmail
                            {
                                EventsId = @event.Id,
                                EventTitle = @event.Title,
                                EventLocation = @event.Location,
                                StartDate = @event.StartDate,
                                GetListDate = @event.ListDate,
                                ParticipantId = db.Participants.Where(p=>p.Email ==participant.Email)
                                                               .Select(p=>p.Id)
                                                               .FirstOrDefault(),
                                ReceiverEmail = participant.Email,
                                SenderName = "scheduleasy.com",
                                SenderEmail = "no-reply@scheduleasy.com",
                                EmailSubject = "Remainder for" + " " + @event.Title
                            };

                //send email
                var emailDelivery = new EmailDeliveryController();
                emailDelivery.DeliverEmail(email, "RemainderEmail").Deliver();
            }
        }
    }
}