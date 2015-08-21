using System.IO;
using System.Web.Hosting;
using System.Web.Mvc;
using Postal;

namespace SchedulerWebApp.Models.PostalEmail
{
    public static class PostalEmailManager
    {
        //create invitation Email 

        public static void SendInvitationEmail(EmailInformation emailInfo)
        {
            var email = ComposeEmail(emailInfo, new InvitationEmail());
            SendCorespondingEmail(email);
        }

        public static void SendChangeInfoEmail(EmailInformation emailInfo)
        {
            var email = ComposeEmail(emailInfo, new EmailInfoChangeEmail());
            SendCorespondingEmail(email);
        }

        public static void SendCancellationEmail(EmailInformation emailInfo)
        {
            var email = ComposeEmail(emailInfo, new CancellationEmail());
            SendCorespondingEmail(email);
        }

        public static void SendEmail(EmailInformation emailInfo, object newEmailObject)
        {
            var email = ComposeEmail(emailInfo, newEmailObject);
            SendCorespondingEmail(email);
        }

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
                           ReceiverEmail = emailInfo.ParticipantEmail,
                           OrganizerName = emailInfo.OrganizerName,
                           OrganizerEmail = emailInfo.OrganizerEmail,
                           SenderEmail = "aim_ahmad@hotmail.com",
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
                            ReceiverEmail = emailInfo.ParticipantEmail,
                            OrganizerName = emailInfo.OrganizerName,
                            OrganizerEmail = emailInfo.OrganizerEmail,
                            SenderEmail = "aim_ahmad@hotmail.com",
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
                            ReceiverEmail = emailInfo.ParticipantEmail,
                            OrganizerName = emailInfo.OrganizerName,
                            OrganizerEmail = emailInfo.OrganizerEmail,
                            SenderEmail = "aim_ahmad@hotmail.com",
                            EmailSubject = emailInfo.CurrentEvent.Title + emailInfo.EmailSubject,
                        };
            }


            return email;
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


        //create Event cancellation Email 
        //send cancellation


        //create remainder Email 
        //send remainder

        //create list Email 
        //send list
    }
}