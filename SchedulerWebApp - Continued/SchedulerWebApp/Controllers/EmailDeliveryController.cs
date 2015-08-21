using System.Net.Mail;
using ActionMailerNext.Mvc5_2;
using SchedulerWebApp.Models.PostalEmail;
using SchedulerWebApp.Models.ViewModels;

namespace SchedulerWebApp.Controllers
{
    public class EmailDeliveryController : MailerBase
    {
        const string SenderName = "scheduleasy.com";
        const string SenderEmail = "no-reply@scheduleasy.com";

        public EmailResult DeliverEmail(InvitationEmail model, string emailTemplate)
        {
            //email Method
            SetMailMethod(MailMethod.SMTP);

            //email Properties

            MailAttributes.From = new MailAddress(SenderEmail, SenderName);
            MailAttributes.To.Add(new MailAddress(model.ReceiverEmail));
            MailAttributes.Subject = model.EmailSubject;

            //view
            return Email(emailTemplate, model);
        }


        public EmailResult DeliverSummary(ParticipantListViewModel model, string emailTemplate)
        {
            //email Method
            SetMailMethod(MailMethod.SMTP);

            //email Properties
            MailAttributes.From = new MailAddress(SenderEmail, SenderName);
            MailAttributes.To.Add(new MailAddress(model.ReceiverEmail));
            MailAttributes.Subject = model.EmailSubject;
            return Email(emailTemplate, model);
        }

        public EmailResult DeliverContactFormula(ContactViewModel model, string emailTemplate)
        {
            //email Method
            SetMailMethod(MailMethod.SMTP);

           var name = model.SenderFistName + " " + model.SenderLastName;

            //email Properties
            MailAttributes.From = new MailAddress(model.SenderEmail, name);
            MailAttributes.To.Add(new MailAddress("aim_ahmad@hotmail.com"));
            MailAttributes.Subject = model.EmailSubject;
            return Email(emailTemplate, model);
        }
    }
}