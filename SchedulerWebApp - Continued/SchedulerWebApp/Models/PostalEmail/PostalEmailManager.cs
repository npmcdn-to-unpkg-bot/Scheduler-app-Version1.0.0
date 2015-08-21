/*
 * Because Postal Doesn't support email Layouts, There are almost simmilar code  
 */
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
            var email = ComposeEmail(emailInfo);

            SendCorespondingEmail(email);
        }

        private static InvitationEmail ComposeEmail(EmailInformation emailInfo)
        {
            var email = new InvitationEmail
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
                            EmailSubject = "Invitation to " + emailInfo.CurrentEvent.Title,
                            ResponseUrl = emailInfo.ResponseUrlString
                        };
            return email;
        }


        private static void SendCorespondingEmail(InvitationEmail email)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var viewpath = Path.GetFullPath(HostingEnvironment.MapPath(@"~/Views/Emails"));
            var engines = new ViewEngineCollection();
            engines.Add(new FileSystemRazorViewEngine(viewpath));
            var emailService = new Postal.EmailService(engines);

            emailService.Send(email);
        }

        //send invitation

        //create Event cancellation Email 
        //send cancellation

        //create Event changed Email 
        //send changed

        //create remainder Email 
        //send remainder

        //create list Email 
        //send list
    }
}