using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using Elmah;
using Postal;
using SchedulerWebApp.Models.DBContext;

namespace SchedulerWebApp.Models.PostalEmail
{
    public static class PostalEmailManager
    {
        static readonly SchedulerDbContext Db = new SchedulerDbContext();

        private static Email ComposeEmail(EmailInformation emailInfo, object newEmailObject)
        {
            Email email = null;

            if (newEmailObject.GetType() == typeof(InvitationEmail))
            {
                email = new InvitationEmail
                       {
                           EventsId = emailInfo.CurrentEvent.Id,
                           EventTitle = emailInfo.CurrentEvent.Title,
                           EventLocation = emailInfo.CurrentEvent.Location,
                           StartDate = emailInfo.CurrentEvent.StartDate,
                           GetListDate = emailInfo.CurrentEvent.ListDate,
                           ParticipantId = emailInfo.ParticipantId,
                           To = emailInfo.ParticipantEmail,
                           OrganizerName = emailInfo.OrganizerName,
                           OrganizerEmail = emailInfo.OrganizerEmail,
                           From = "aim_ahmad@hotmail.com",
                           EmailSubject = emailInfo.CurrentEvent.Title + emailInfo.EmailSubject,
                           ResponseUrl = emailInfo.ResponseUrl
                       };

            }
            else if (newEmailObject.GetType() == typeof(EmailInfoChangeEmail))
            {
                email = new EmailInfoChangeEmail
                        {
                            EventsId = emailInfo.CurrentEvent.Id,
                            EventTitle = emailInfo.CurrentEvent.Title,
                            EventLocation = emailInfo.CurrentEvent.Location,
                            StartDate = emailInfo.CurrentEvent.StartDate,
                            GetListDate = emailInfo.CurrentEvent.ListDate,
                            ParticipantId = emailInfo.ParticipantId,
                            To = emailInfo.ParticipantEmail,
                            OrganizerName = emailInfo.OrganizerName,
                            OrganizerEmail = emailInfo.OrganizerEmail,
                            From = "aim_ahmad@hotmail.com",
                            EmailSubject = emailInfo.CurrentEvent.Title + emailInfo.EmailSubject,
                            ResponseUrl = emailInfo.ResponseUrl,
                            EventDetailsUrl = emailInfo.EventDetailsUrl
                        };
            }
            else if (newEmailObject.GetType() == typeof(CancellationEmail))
            {
                email = new CancellationEmail
                        {
                            EventsId = emailInfo.CurrentEvent.Id,
                            EventTitle = emailInfo.CurrentEvent.Title,
                            EventLocation = emailInfo.CurrentEvent.Location,
                            StartDate = emailInfo.CurrentEvent.StartDate,
                            GetListDate = emailInfo.CurrentEvent.ListDate,
                            ParticipantId = emailInfo.ParticipantId,
                            To = emailInfo.ParticipantEmail,
                            OrganizerName = emailInfo.OrganizerName,
                            OrganizerEmail = emailInfo.OrganizerEmail,
                            From = "aim_ahmad@hotmail.com",
                            EmailSubject = emailInfo.CurrentEvent.Title + emailInfo.EmailSubject
                        };
            }


            return email;
        }

        public static void SendListEmail(EmailInformation emailInfo, object emailObject)
        {
            var currentEvent = GetCurrentEvent(emailInfo);

            ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(new Exception("Testing")));

            if (currentEvent == null) return;

            var participants = currentEvent.Participants.ToList();

            var allParticipant = participants.Count();
            var respondedParticipants = participants.Count(p => p.Responce);
            var attendingParticipant = participants.Count(p => p.Availability);
            var notAttendingParticipant = allParticipant - attendingParticipant;

            Email email = new ParticipantListEmail
                          {
                              EventTitle = emailInfo.CurrentEvent.Title,
                              From = "test@email.com",
                              To = emailInfo.OrganizerEmail,
                              OrganizerName = emailInfo.OrganizerName,
                              AllParticipants = allParticipant,
                              ParticipantAttending = attendingParticipant,
                              ParticipantNotAttending = notAttendingParticipant,
                              ParticipantsResponded = respondedParticipants,
                              EmailSubject = "Participants summary" + " " + emailInfo.CurrentEvent.Title,
                              EventDetailsUrl = emailInfo.EventDetailsUrl
                          };

            SendCorespondingEmail(email);
        }

        public static void SendRemainder(EmailInformation emailInfo, object emailObject)
        {
            var @event = GetCurrentEvent(emailInfo);
            var noResponseParticipants = @event.Participants
                                         .Where(p => p.Responce == false).ToList();

            foreach (var participant in noResponseParticipants)
            {
                //create email
                Email email = new RemainderEmail
                              {
                                  EventTitle = emailInfo.CurrentEvent.Title,
                                  EventLocation = emailInfo.CurrentEvent.Location,
                                  StartDate = emailInfo.CurrentEvent.StartDate,
                                  GetListDate = emailInfo.CurrentEvent.ListDate,
                                  To = participant.Email,
                                  From = "no-reply@scheduleasy.com",
                                  EmailSubject = "Remainder for" + " " + emailInfo.CurrentEvent.Title,
                                  ResponseUrl = emailInfo.ResponseUrl
                              };
                SendCorespondingEmail(email);
            }
        }

        public static void SendEmail(EmailInformation emailInfo, object newEmailObject)
        {
            var email = ComposeEmail(emailInfo, newEmailObject);
            var emailAttachment = Service.CreateAttachment(emailInfo);
            email.Attach(emailAttachment);
            SendCorespondingEmail(email);
        }

        public static void SendContactUsEmail(ContactUsEmail email)
        {
            SendCorespondingEmail(email);
        }

        private static void SendCorespondingEmail(Email email)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var viewpath = Path.GetFullPath(HostingEnvironment.MapPath(@"~/Views/Emails"));
            var engines = new ViewEngineCollection();
            engines.Add(new FileSystemRazorViewEngine(viewpath));
            var emailService = new Postal.EmailService(engines); 
     
            emailService.Send(email);
        }

        private static Event GetCurrentEvent(EmailInformation emailInfo)
        {
            var currentEvent = Db.Events.Where(e => e.Id == emailInfo.CurrentEvent.Id)
                .Include(e => e.Participants)
                .FirstOrDefault();

            return currentEvent;
        }
    }
}