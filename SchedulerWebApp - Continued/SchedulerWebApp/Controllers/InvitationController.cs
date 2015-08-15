using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using SchedulerWebApp.Models;
using SchedulerWebApp.Models.DBContext;
using SchedulerWebApp.Models.Service;
using SchedulerWebApp.Models.ViewModels;

namespace SchedulerWebApp.Controllers
{
    public class InvitationController : Controller
    {
        private readonly SchedulerDbContext _db = new SchedulerDbContext();

        #region Send Ivitation

        public ActionResult SendEventsInvitation(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var @event = GetUserEvents().Find(e => e.Id == id);

            if (@event == null)
            {
                return View("_NoAccess");
            }

            var todayDate = DateTime.UtcNow.Date;
            var eventsDate = @event.StartDate.Date;

            bool hasPassed = eventsDate < todayDate;
            return hasPassed ? View("_CantInvite", @event) : View(ReturnInvitationModel(id));
        }

        [HttpPost]
        public ActionResult SendEventsInvitation([Bind(Include = "ParticipantsEmails,EventId")] InvitationViewModel model
            /*int? id*/)
        {
            var id = model.EventId;
            //check input values
            if (!ModelState.IsValid)
            {
                return View(ReturnInvitationModel(model.EventId));
            }

            // get event from database
            var eventForInvitation = _db.Events.Find(model.EventId);


            var todayDate = DateTime.UtcNow.Date;
            var eventEndDate = eventForInvitation.EndDate.Date;

            //check if event has happened
            bool notPassed = todayDate <= eventEndDate;

            if (!notPassed)
            {
                return View("_CantInvite", eventForInvitation);
            }

            var userid = User.Identity.GetUserId();
            var user = _db.Users.Find(userid);

            List<string> emailList = model.ParticipantsEmails.Split(',').ToList();

            //loop through emails
            foreach (var participantEmail in emailList)
            {
                //create new participant 
                var invitedParticipant = new Participant
                                         {
                                             Email = participantEmail,
                                             Responce = false,
                                             Availability = false
                                         };

                //save participant in coresponding event
                eventForInvitation.Participants.Add(invitedParticipant);
                _db.SaveChanges();

                #region Create and send Email
                    
                //create email
                var email = new EmailViewModel
                            {
                                EventsId = eventForInvitation.Id,
                                EventTitle = eventForInvitation.Title,
                                EventLocation = eventForInvitation.Location,
                                StartDate = eventForInvitation.StartDate,
                                GetListDate = eventForInvitation.ListDate,
                                ParticipantId = eventForInvitation.Participants.Where(p => p.Email == participantEmail)
                                    .Select(p => p.Id)
                                    .FirstOrDefault(),
                                OrganizerName = user.FirstName,
                                OrganizerEmail = user.UserName,
                                SenderEmail = "aim_ahmad@hotmail.com",
                                ReceiverEmail = participantEmail,
                                EmailSubject = "Invitation to " + eventForInvitation.Title
                            };

                //Send email
                new EmailDeliveryController().DeliverEmail(email, "InvitationEmail").Deliver();

                #endregion

                var eventId = eventForInvitation.Id;
                var reminderDate = eventForInvitation.ReminderDate;
                var listDate = eventForInvitation.ListDate;

                // start remainder scheduler
                var reminderScheduler = new ReminderScheduler();
                reminderScheduler.Start(reminderDate, eventId, user.FirstName, user.UserName);

                // start participant list summary scheduler
                var listScheduler = new ParticipantSummaryScheduler();
                listScheduler.Start(eventId, user.FirstName, user.Email, listDate);
            }
            //redirect to details
            return RedirectToAction("Details", "Events", new { id });

        }

        #endregion
        
        #region No double Invitation

        public JsonResult CheckParticipantEmail(string participantsEmails, int eventId)
        {
            List<string> emails = participantsEmails.Split(',').ToList();
            Event _event = _db.Events.Find(eventId);
            bool isNotyetInvited = false;

            foreach (string email in emails)
            {
                string thisEmail = email;
                isNotyetInvited = _event.Participants.All(p => p.Email != thisEmail);

                if (!isNotyetInvited)
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(isNotyetInvited, JsonRequestBehavior.AllowGet);
        }

        #endregion

        private List<Event> GetUserEvents()
        {
            var currentUser = User.Identity.GetUserId();
            var userEvents = _db.Users.Find(currentUser).Events;
            return userEvents;
        }

        public InvitationViewModel ReturnInvitationModel(int? id)
        {
            var @event = GetUserEvents().Find(e => e.Id == id);
            var invitationViewModel = new InvitationViewModel
                                      {
                                          EventId = @event.Id,
                                          EventTitle = @event.Title,
                                          EventDate = @event.StartDate,
                                          EventLocation = @event.Location
                                      };
            return invitationViewModel;
        }
    }
}