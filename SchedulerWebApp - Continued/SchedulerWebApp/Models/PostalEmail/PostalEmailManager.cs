using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using System.Web.UI;
using Elmah;
using Postal;
using SchedulerWebApp.Models.DBContext;

namespace SchedulerWebApp.Models.PostalEmail
{
    public static class PostalEmailManager
    {
        static readonly SchedulerDbContext Db = new SchedulerDbContext();

        private static Email ComposeDynamicEmail(EmailInformation emailInfo, object newEmailObject)
        {
            dynamic email = null;

            if (newEmailObject.GetType() == typeof(InvitationEmail))
            {
                email = ComposeEmail(emailInfo, "Invitation");

                //Attach a file
                AddAttachAttachment(email, emailInfo);
            }
            else if (newEmailObject.GetType() == typeof(EmailInfoChangeEmail))
            {
                //write changed Info email
                email = ComposeEmail(emailInfo, "EmailInfoChange");

                //Attach a file
                AddAttachAttachment(email, emailInfo);
            }
            else if (newEmailObject.GetType() == typeof(CancellationEmail))
            {
                //Write Cancellation Email
                email = ComposeEmail(emailInfo, "Cancellation");
            }

            return email;
        }

        public static void SendEmail(EmailInformation emailInfo, object newEmailObject)
        {
            var email = ComposeDynamicEmail(emailInfo, newEmailObject);

            SendCorespondingEmail(email);
        }

        public static void SendCorespondingEmail(Email email)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            /*var viewpath = Path.GetFullPath(HostingEnvironment.MapPath(@"~/Views/Emails"));
            var engines = new ViewEngineCollection();
            engines.Add(new FileSystemRazorViewEngine(viewpath));
            var emailService = new Postal.EmailService(engines);*/

            #region Setting Credentials using smtp client class instead of in web.config

            var client = new SmtpClient();
            var credential = new NetworkCredential("aim.mangapi@gmail.com", "mangapi.");
            client.UseDefaultCredentials = false;
            client.Credentials = credential;
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network; 

            var emailService = new Postal.EmailService(ViewEngines.Engines, () => client);

            #endregion

            try
            {
                //sending email using web.config credentials
                email.Send();

                //sending email using above credentials
                //emailService.Send(email);
            }
            catch (Exception exception)
            {
                ErrorSignal.FromCurrentContext().Raise(exception);
            }

        }

        private static void AddAttachAttachment(dynamic email, EmailInformation emailInfo)
        {
            var emailAttachment = Service.CreateAttachment(emailInfo);
            email.Attach(emailAttachment);
        }

        public static void SendListEmail(EmailInformation emailInfo, object emailObject)
        {
            var currentEvent = GetCurrentEvent(emailInfo);

            if (currentEvent == null)
            {
                //using Error log Class
                const string errorMessage = "Event returned null, no list of participants have been sent to the Organizer";
                ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(new Exception(errorMessage)));

                return;
            }

            var participants = currentEvent.Participants.ToList();

            var allParticipant = participants.Count();
            var respondedParticipants = participants.Count(p => p.Responce);
            var attendingParticipant = participants.Count(p => p.Availability);
            var notAttendingParticipant = allParticipant - attendingParticipant;

            dynamic email = new Email("ParticipantList");

            email.To = emailInfo.OrganizerEmail;

            //ToDo:change the email of the sender
            email.From = "no-reply@scheduleasy.com";
            email.EmailSubject = "Participants summary" + " " + emailInfo.CurrentEvent.Title;

            email.EventTitle = emailInfo.CurrentEvent.Title;

            email.OrganizerName = emailInfo.OrganizerName;

            email.AllParticipants = allParticipant;
            email.ParticipantAttending = attendingParticipant;
            email.ParticipantNotAttending = notAttendingParticipant;
            email.ParticipantsResponded = respondedParticipants;
            email.EventDetailsUrl = emailInfo.EventDetailsUrl;
            email.attendindParticipants = participants.Where(p=>p.Availability);

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
                            + "Error msg is " + exception.Message)));
            }


            foreach (var participant in noResponseParticipants)
            {
                var p = participant;
                var responseUrl = emails.Where(e => e.ParticipantId == p.Id)
                                        .Select(e => e.ResponseUrl)
                                        .FirstOrDefault();

                //create email
                dynamic email = new Email("Remainder");

                email.To = participant.Email;

                //TOdo: Add the right sender email
                email.From = "no-reply@scheduleasy.com";
                email.EmailSubject = "Remainder for" + " " + emailInfo.CurrentEvent.Title;

                email.EventTitle = emailInfo.CurrentEvent.Title;
                email.EventLocation = emailInfo.CurrentEvent.Location;

                email.StartDate = emailInfo.CurrentEvent.StartDate.GetValueOrDefault();
                email.GetListDate = emailInfo.CurrentEvent.ListDate.GetValueOrDefault();

                email.ResponseUrl = responseUrl;


                //send email
                SendCorespondingEmail(email);
            }

        }

        public static void SendContactUsEmail(ContactUsEmail contactUsEmail)
        {
            dynamic email = new Email("ContactUs");

            //TODO: Go to the email and confirm receiver email address 
            email.To = contactUsEmail.To;
            email.From = contactUsEmail.From;
            email.EmailSubject = contactUsEmail.EmailSubject;

            email.SenderFistName = contactUsEmail.SenderFistName + " " + contactUsEmail.SenderLastName;

            email.EmailBody = contactUsEmail.EmailBody;

            SendCorespondingEmail(email);
        }

        public static void SendResetPassword(PasswordResetEmail resetEmail)
        {
            dynamic email = new Email("ResetPassword");

            email.To = resetEmail.ReceiverEmail;

            //ToDo: Confirm receiver enail address
            email.From = resetEmail.AdminEmail;
            email.EmailSubject = resetEmail.EmailSubject;

            email.Name = resetEmail.ReceiverName;
            email.ResetLink = resetEmail.PassWordRestLink;

            SendCorespondingEmail(email);
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

        private static dynamic ComposeEmail(EmailInformation emailInfo, string emailView)
        {

            dynamic email = new Email(emailView);

            email.To = emailInfo.ParticipantEmail;
            email.From = emailInfo.OrganizerEmail;
            email.Subject = emailInfo.CurrentEvent.Title + emailInfo.EmailSubject;

            email.EventTitle = emailInfo.CurrentEvent.Title;
            email.EventLocation = emailInfo.CurrentEvent.Location;
            email.EventsId = emailInfo.CurrentEvent.Id;

            email.StartDate = emailInfo.CurrentEvent.StartDate.GetValueOrDefault();
            email.GetListDate = emailInfo.CurrentEvent.ListDate.GetValueOrDefault();

            email.ParticipantId = emailInfo.ParticipantId;
            email.OrganizerName = emailInfo.OrganizerName;

            email.ResponseUrl = emailInfo.ResponseUrl;

            email.EventDetailsUrl = emailInfo.EventDetailsUrl;
            return email;
        }
    }
}