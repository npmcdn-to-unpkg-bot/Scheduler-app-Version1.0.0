using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
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
        public ActionResult SendEventsInvitation([Bind(Include = "ParticipantsEmails,EventDate,EventId,SendRemainder,ReminderDate,ListDate")] InvitationViewModel model)
        {
            var id = model.EventId;

            if (!ModelState.IsValid)
            {
                return View(ReturnInvitationModel(id));
            }

            // get event from database
            var eventForInvitation = _db.Events.Find(id);
            eventForInvitation.ListDate = model.ListDate;
            eventForInvitation.ReminderDate = model.ReminderDate;

            //Check if there is Participants
            //var noInvitation = !eventForInvitation.Participants.ToList().Any();

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
            var emails = new List<EmailInformation>();

            //loop through emails
            var emailList = model.ParticipantsEmails.Split(',').ToList();

            foreach (var participantEmail in emailList)
            {
                var email = Service.RemoveBrackets(participantEmail);

                //create new participant 
                var invitedParticipant = new Participant
                                         {
                                             Email = email,
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
                                                       .Where(p => p.Email == email)
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
                                    ParticipantEmail = email,
                                    ResponseUrl = responseUrl,
                                    RemainderDate = eventForInvitation.ReminderDate.GetValueOrDefault(),
                                    ListDate = eventForInvitation.ListDate.GetValueOrDefault(),
                                    EventDetailsUrl = detailsUrl,
                                    EmailSubject = " Invitation"
                                };

                emails.Add(emailInfo);

                //Send Invitation Email
                try
                {
                    PostalEmailManager.SendEmail(emailInfo, new InvitationEmail());
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Email sent to " + emailInfo.ParticipantEmail));

                    if (model.SendRemainder)
                    {
                        var remainderDate = Service.GetRemanderDate(eventForInvitation);
                        JobManager.ScheduleRemainderEmail(emails, remainderDate);
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("remainder is set at " + remainderDate));
                    }
                }
                catch (Exception exception)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(exception);
                    return RedirectToAction("Error");
                }


                #endregion

                #region after sending email, save unsaved contacts

                var contactEmails = _contactsController.GetUserContacts(GetUserId());
                allSaved = contactEmails.Any(c => c.Email == email);

                if (allSaved)
                {
                    continue;
                }
                var contact = new Contact { Email = email };
                contacts.Add(contact);
                unsavedContacts.Contacts = contacts;

                #endregion

            }

            #region Scheduling remainder emails

            // start participant list summary scheduler
            var listDate = Service.GetListDate(eventForInvitation);
            JobManager.ScheduleParticipantListEmail(emailInfo, listDate);

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

        /// <summary>
        /// Todo: Refactor this to a service class
        /// </summary>
        /// <param name="eventForInvitation"></param>
        /// <returns></returns>
        private static bool EventHasNotPassed(Event eventForInvitation)
        {
            //check if event has happened

            /*var todayDate = DateTime.UtcNow.Date;
            var eventEndDate = eventForInvitation.StartDate.GetValueOrDefault().Date;
            //notPassed = todayDate <= eventEndDate;*/


            var todaysDate = DateTime.UtcNow.ToLocalTime();
            var compareDates = eventForInvitation.StartDate.GetValueOrDefault().CompareTo(todaysDate);

            bool notPassed = compareDates >= 0;

            return notPassed;
        }

        private string GetUserId()
        {
            return User.Identity.GetUserId();
        }

        /*public ActionResult RemoveContact(UnsavedContactViewModel viewModel)
                {
                    //return View("unsavedContacts", model);

                    //var returnUrl = new UrlHelper(Request.RequestContext).Action();
                    return Json(new { result = "Redirect", url = Url.Action("SaveEmails", "Invitation", viewModel) }, JsonRequestBehavior.AllowGet);
                }
         */

        /// <summary>
        /// Called when there are emaills that are not saved
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveEmails()
        {
            var viewmodel = (UnsavedContactViewModel)TempData["model"];
            if (!ModelState.IsValid)
            {
                return View("unsavedContacts", viewmodel);
            }

            if (viewmodel == null)
            {
                return RedirectToAction("Index", "Events");
            }

            return View("unsavedContacts", viewmodel);
        }

        public ActionResult Error()
        {
            return View("Error");
        }

        #region No double Invitation

        public JsonResult CheckParticipantEmail(string participantsEmails, int eventId)
        {
            var emails = participantsEmails.Split(',').ToList();
            var _event = _db.Events.Find(eventId);
            var isNotyetInvited = false;

            foreach (var email in emails)
            {
                var thisEmail = Service.RemoveBrackets(email);

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

            var eventListDate = @event.ListDate;
            var listDate = Service.SetCorectDate(eventListDate);

            var eventRemainderDate = @event.ReminderDate;
            var remainderDate = Service.SetCorectDate(eventRemainderDate);
            var eventDate = @event.StartDate ?? DateTime.Now;

            var invitationViewModel = new InvitationViewModel
                                      {
                                          EventId = @event.Id,
                                          EventTitle = @event.Title,
                                          EventDate = eventDate,
                                          EventLocation = @event.Location,
                                          ListDate = listDate,
                                          ReminderDate = remainderDate,
                                          SendRemainder = true
                                      };
            return invitationViewModel;
        }
    }
}