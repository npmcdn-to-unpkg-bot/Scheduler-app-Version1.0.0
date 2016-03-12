using System;
using System.Collections.Generic;
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
                           StartDate = emailInfo.CurrentEvent.StartDate.GetValueOrDefault(),
                           GetListDate = emailInfo.CurrentEvent.ListDate.GetValueOrDefault(),
                           ParticipantId = emailInfo.ParticipantId,
                           To = emailInfo.ParticipantEmail,
                           OrganizerName = emailInfo.OrganizerName,
                           OrganizerEmail = emailInfo.OrganizerEmail,
                           From = emailInfo.OrganizerEmail, //this has to schedule easy info email
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
                            StartDate = emailInfo.CurrentEvent.StartDate.GetValueOrDefault(),
                            GetListDate = emailInfo.CurrentEvent.ListDate.GetValueOrDefault(),
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
                            StartDate = emailInfo.CurrentEvent.StartDate.GetValueOrDefault(),
                            GetListDate = emailInfo.CurrentEvent.ListDate.GetValueOrDefault(),
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
            
            if (currentEvent == null)
            {
                ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(
                    new Error(new Exception("participant email of an event has not been sent, The event has returned NULL"))
                    );
                return;
            }

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

        public static void SendRemainder(List<EmailInformation> emails, object emailObject)
        {
            var emailInfo = emails.First();

            var noResponseParticipants = new List<Participant>();

            var @event = GetCurrentEvent(emailInfo);
            try
            {
                noResponseParticipants = @event.Participants
                                         .Where(p => p.Responce == false).ToList();
            }
            catch (Exception exception)
            {

                ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(
                    new Error(
                        new Exception("participant remainder emails were not sent, The event has returned NULL. " 
                            +"Error msg is " + exception.Message)));
            }
            

            foreach (var participant in noResponseParticipants)
            {
                var p = participant;
                var responseUrl = emails.Where(e => e.ParticipantId == p.Id)
                                        .Select(e => e.ResponseUrl)
                                        .FirstOrDefault();

                //create email
                Email email = new RemainderEmail
                              {
                                  EventTitle = emailInfo.CurrentEvent.Title,
                                  EventLocation = emailInfo.CurrentEvent.Location,
                                  StartDate = emailInfo.CurrentEvent.StartDate.GetValueOrDefault(),
                                  GetListDate = emailInfo.CurrentEvent.ListDate.GetValueOrDefault(),
                                  To = participant.Email,
                                  From = "no-reply@scheduleasy.com",
                                  EmailSubject = "Remainder for" + " " + emailInfo.CurrentEvent.Title,
                                  ResponseUrl = responseUrl
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

        public static void SendCorespondingEmail(Email email)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            /*var viewpath = Path.GetFullPath(HostingEnvironment.MapPath(@"~/Views/Emails"));
            var engines = new ViewEngineCollection();
            engines.Add(new FileSystemRazorViewEngine(viewpath));
            var emailService = new Postal.EmailService(engines);*/

            dynamic email1 = new Email("Test");
            email1.To = "s.buchumi@gmail.com";
            email1.From = "admin@me.com";
            email1.EmailSubject = "Email subject";
            email1.EmailBody = "testemail from contact";
            email1.SenderFistName = "Tester 1";

            email1.Send();
        }

        private static Event GetCurrentEvent(EmailInformation emailInfo)
        {
            Event currentEvent = null;
            try
            {
                currentEvent = Db.Events.Where(e => e.Id == emailInfo.CurrentEvent.Id)
                    .Include(e => e.Participants)
                    .FirstOrDefault();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(new Exception(exception.Message)));
            }

            return currentEvent;
        }
    }
}