﻿using System.Data.Entity;
using System.Linq;
using SchedulerWebApp.Controllers;
using SchedulerWebApp.Models.DBContext;
using SchedulerWebApp.Models.ViewModels;

namespace SchedulerWebApp.Models.Service
{
    public class EmailDelivery
    {
        readonly SchedulerDbContext _db = new SchedulerDbContext();

        public void SendInfoEmail(int eventId, string organizerName, string organizerEmail, string emailSubject, string emailTemplate)
        {
            var @event = _db.Events
                            .Where(e => e.Id == eventId)
                            .Include(e => e.Participants)
                            .FirstOrDefault();

            if (@event == null) return;

            var participants = @event.Participants.ToList();

            foreach (var participant in participants)
            {
                //create email
                var email = new InvitationEmail
                            {
                                EventsId = @event.Id,
                                EventTitle = @event.Title,
                                EventLocation = @event.Location,
                                StartDate = @event.StartDate,
                                GetListDate = @event.ListDate,
                                ParticipantId = @event.Participants.Where(p => p.Email == participant.Email)
                                    .Select(p => p.Id)
                                    .FirstOrDefault(),
                                SenderName = "scheduleasy.com",
                                SenderEmail = "no-reply@scheduleasy.com",
                                OrganizerName = organizerName,
                                OrganizerEmail = organizerEmail,
                                ReceiverEmail = participant.Email,
                                EmailSubject = @event.Title + " " + emailSubject
                            };

                //send email
                new EmailDeliveryController().DeliverEmail(email, emailTemplate).Deliver();
            }
        }

        public void SendContactFormulaEmail(ContactViewModel model,string emailTemplate)
        {
            var email = new ContactViewModel()
                        {
                            SenderFistName = model.SenderFistName,
                            SenderLastName = model.SenderLastName,
                            SenderEmail = model.SenderEmail,
                            EmailSubject = model.EmailSubject,
                            EmailBody = model.EmailBody
                        };
            new EmailDeliveryController().DeliverContactFormula(email, emailTemplate).Deliver();
        }

        
        
    }
}