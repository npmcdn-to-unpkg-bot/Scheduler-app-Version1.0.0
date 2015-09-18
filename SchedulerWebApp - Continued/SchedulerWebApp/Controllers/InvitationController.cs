﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Hangfire;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using SchedulerWebApp.Models;
using SchedulerWebApp.Models.DBContext;
using SchedulerWebApp.Models.HangfireJobs;
using SchedulerWebApp.Models.PostalEmail;
using SchedulerWebApp.Models.ViewModels;

namespace SchedulerWebApp.Controllers
{
    public class InvitationController : Controller
    {
        private readonly ContactsController _contactsController = new ContactsController();
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
            bool hasPassed = EventHasNotPassed(@event);
            return !hasPassed ? View("_CantInvite", @event) : View(ReturnInvitationModel(id));
        }

        [HttpPost]
        public ActionResult SendEventsInvitation([Bind(Include = "ParticipantsEmails,EventId,SendRemainder")] InvitationViewModel model)
        {
            var id = model.EventId;
            //check input values
            if (!ModelState.IsValid)
            {
                return View(ReturnInvitationModel(id));
            }

            // get event from database
            var eventForInvitation = _db.Events.Find(id);

            //Check if there is Participants
            var noInvitation = !eventForInvitation.Participants.ToList().Any();
            

            //Check if invitations can still be sent
            var notPassed = EventHasNotPassed(eventForInvitation);
            if (!notPassed)
            {
                return View("_CantInvite", eventForInvitation);
            }

            var userid = User.Identity.GetUserId();
            var user = _db.Users.Find(userid);

            var unsavedContacts = new UnsavedContactViewModel();
            EmailInformation emailInfo = null;
            var allSaved = false;
            var contacts = new List<Contact>();

            //loop through emails
            var emailList = model.ParticipantsEmails.Split(',').ToList();

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

                var organizerFirstName = user.FirstName;
                var organizerEmail = user.Email;

                var participantId = eventForInvitation.Participants
                                                       .Where(p => p.Email == participantEmail)
                                                       .Select(p => p.Id)
                                                       .FirstOrDefault();

                var responseUrl = Url.Action("Response", "Response", new RouteValueDictionary(new { id = eventForInvitation.Id, pId = participantId }), "https");
                var detailsUrl = Url.Action("Details", "Events", new RouteValueDictionary(new { id = eventForInvitation.Id }), "https");


                emailInfo = new EmailInformation
                                {
                                    CurrentEvent = eventForInvitation,
                                    OrganizerEmail = organizerEmail,
                                    OrganizerName = organizerFirstName,
                                    ParticipantId = participantId,
                                    ParticipantEmail = participantEmail,
                                    ResponseUrl = responseUrl,
                                    RemainderDate = eventForInvitation.ReminderDate,
                                    ListDate = eventForInvitation.ListDate,
                                    EventDetailsUrl = detailsUrl,
                                    EmailSubject = " Invitation"
                                };

                //Send Invitation Email
                PostalEmailManager.SendEmail(emailInfo, new InvitationEmail());

                #endregion
                
                //after sending email its time to save unsaved contacts
                var contactEmails = _contactsController.GetUserContacts(GetUserId());
                allSaved = contactEmails.Any(c => c.Email == participantEmail);

                if (allSaved)
                {
                    continue;
                }
                var contact = new Contact { Email = participantEmail };
                contacts.Add(contact);
                unsavedContacts.Contacts = contacts;
            }

            #region Scheduling emails
            
            if (noInvitation)
            {
                
                //organizer wants to send remainders
                if (model.SendRemainder)
                {
                    JobManager.ScheduleRemainderEmail(emailInfo);
                }
                
                // start participant list summary scheduler
                JobManager.ScheduleParticipantListEmail(emailInfo);
                JobManager.AddJobsIntoEvent(id);
            }

            #endregion

            //redirect to details if all contacts are saved
            if (allSaved)
            {
                return RedirectToAction("Details", "Events", new { id });
            }

            //ask user to save in his contacts return view with list of unsaved contacts
            unsavedContacts.EventId = eventForInvitation.Id;
            TempData["model"] = unsavedContacts;           //Pass list to SaveEmails action
            return RedirectToAction("SaveEmails");
        }

        

        #endregion

        private static bool EventHasNotPassed(Event eventForInvitation)
        {
            var todayDate = DateTime.UtcNow.Date;
            var eventEndDate = eventForInvitation.StartDate.Date;

            //check if event has happened
            bool notPassed = todayDate <= eventEndDate;
            return notPassed;
        }

        private string GetUserId()
        {
            return User.Identity.GetUserId();
        }

        /// <summary>
        /// called when there are emaills that are not saved
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveEmails()
        {
            var viewmodel = (UnsavedContactViewModel)TempData["model"];
            return View("unsavedContacts", viewmodel);
        }

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
                                          EventLocation = @event.Location,
                                          SendRemainder = true
                                      };
            return invitationViewModel;
        }
    }
}