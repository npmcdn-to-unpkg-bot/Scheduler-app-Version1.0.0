using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
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
        private readonly Service _service = new Service();

        private string UserId
        {
            get { return User.Identity.GetUserId(); }
        }

        #region Send Ivitation

        public ActionResult SendEventsInvitation(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }
            var @event = _service.GetUserSpecificEvent(id);

            if (@event == null)
            {
                return View("_NoAccess");
            }
            bool hasPassed = _service.EventHasNotPassed(@event);
            return !hasPassed ? View("_CantInvite", @event) : View(ReturnInvitationModel(id));
        }

        [HttpPost]
        public ActionResult SendEventsInvitation([Bind(Include =
            "ParticipantsEmails,EventDate,EventId,SendRemainder,ReminderDate,ListDate")] InvitationViewModel model)
        {
            var id = model.EventId;

            if (!ModelState.IsValid)
            {
                return View(ReturnInvitationModel(id));
            }

            // get event from database
            var eventForInvitation = GetEvent(id);
            eventForInvitation.ListDate = model.ListDate;
            eventForInvitation.ReminderDate = model.ReminderDate;

            /*
             * This is used to check time on the server 
             * when this is deployed
             */
            Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("{ From Invitation controller sending invitation } Remainder date is on " + model.ReminderDate));
            Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("{ From Invitation controller sending invitation } List date is on " + model.ListDate));


            //Check if invitations can still be sent
            var notPassed = _service.EventHasNotPassed(eventForInvitation);
            if (!notPassed)
            {
                return View("_CantInvite", eventForInvitation);
            }

            var user = _service.GetUser();

            var unsavedContacts = new UnsavedContactViewModel();
            EmailInformation emailInfo = null;
            var allSaved = false;
            var contacts = new List<Contact>();
            var emails = new List<EmailInformation>();

            //loop through emails
            var emailList = model.ParticipantsEmails.Split(',').ToList();

            foreach (var participantEmail in emailList)
            {
                var email = _service.RemoveBrackets(participantEmail);

                #region create and save new participant

                var invitedParticipant = _service.CreateParticipant(email);
                eventForInvitation.Participants.Add(invitedParticipant);
                _db.SaveChanges();

                #endregion

                #region Create and send Email

                var participantId = eventForInvitation.Participants
                                                       .Where(p => p.Email == email)
                                                       .Select(p => p.Id)
                                                       .FirstOrDefault();

                var responseUrl = CreateUrl("Response", "Response", eventForInvitation, participantId);
                var detailsUrl = CreateUrl("Details", "Events", eventForInvitation, 0);


                emailInfo = new EmailInformation
                                {
                                    CurrentEvent = eventForInvitation,
                                    OrganizerEmail = user.Email,
                                    OrganizerName = user.FirstName,
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

                var contactEmails = _contactsController.GetUserContacts(UserId);
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

            #region Scheduling List email

            // start participant list summary scheduler
            var listDate = Service.GetListDate(eventForInvitation);
            JobManager.ScheduleParticipantListEmail(emailInfo, listDate);

            #endregion

            //redirect to details if all contacts are saved
            if (allSaved)
            {
                Response.Cookies.Add(new HttpCookie("successCookie", "Action is completed successfully"));

                return RedirectToAction("Details", "Events", new { id });
            }

            //Let user save his contacts return view with list of unsaved contacts
            unsavedContacts.EventId = eventForInvitation.Id;
            TempData["model"] = unsavedContacts;           //Pass list to SaveEmails action
            return RedirectToAction("SaveEmails");
        }

        private string CreateUrl(string actionName, string controllerName, Event eventForInvitation, int participantId)
        {
            if (participantId == 0)
            {
                return Url.Action(actionName, controllerName, new RouteValueDictionary(new { id = eventForInvitation.Id }), "https");
            }
            return Url.Action(actionName, controllerName, new RouteValueDictionary(new { id = eventForInvitation.Id, pId = participantId }), "https");
        }

        #endregion


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
            var _event = GetEvent(eventId);
            var isNotyetInvited = false;

            foreach (var email in emails)
            {
                var thisEmail = _service.RemoveBrackets(email);

                isNotyetInvited = _event.Participants.All(p => p.Email != thisEmail);

                if (!isNotyetInvited)
                {
                    return Json(String.Format("{0} is already invited", thisEmail), JsonRequestBehavior.AllowGet);
                }
            }

            return Json(isNotyetInvited, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public InvitationViewModel ReturnInvitationModel(int? id)
        {
            var @event = _service.GetUserSpecificEvent(id);

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
                                          SendRemainder = false
                                      };
            return invitationViewModel;
        }

        private Event GetEvent(int id)
        {
            return _db.Events.Find(id);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}