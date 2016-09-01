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
using SchedulerWebApp.Models.ValidationAttributes;
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

            //ToDo: change list and remainder dates to swiss time to be used for sending email

            // get event from database
            var eventForInvitation = GetEvent(id);
            eventForInvitation.ListDate =
            // ReSharper disable once PossibleInvalidOperationException
                ConvertDateTime.ToSwissTimezone(TimeZoneInfo.ConvertTimeToUtc((DateTime) model.ListDate));

            if (model.ReminderDate != null)
            {
                eventForInvitation.ReminderDate = ConvertDateTime.ToSwissTimezone(TimeZoneInfo.ConvertTimeToUtc((DateTime)model.ReminderDate));
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("{ From Invitation controller sending invitation } Remainder date is on " + eventForInvitation.ReminderDate));
            }

            /*
             * This is used to check time on the server 
             * when this is deployed
             */
           
            Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("{ From Invitation controller sending invitation } List date is on " + eventForInvitation.ListDate));


            //Check if invitations can still be sent
            var notPassed = _service.EventHasNotPassed(eventForInvitation);
            if (!notPassed)
            {
                return View("_CantInvite", eventForInvitation);
            }

            

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

                //save new participant
                SaveParticipantInDb(email, eventForInvitation);


                #region Create and send Email

                emailInfo = ComposeEmailInfo(eventForInvitation, email);
                emails.Add(emailInfo);

                //Send Invitation Email
                try
                {
                    PostalEmailManager.SendEmail(emailInfo, new InvitationEmail());

                    //todo: this is to be removed before deployment for production
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Email sent to " + emailInfo.ParticipantEmail));

                    if (model.SendRemainder)
                    {
                        var remainderDate = Service.GetRemanderDate(eventForInvitation);
                        JobManager.ScheduleRemainderEmail(emails, remainderDate);

                        //todo: this is to be removed before deployment for production
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

        private EmailInformation ComposeEmailInfo(Event eventForInvitation, 
            string email)
        {

            var user = _service.GetUser();
            var participantId = GetParticipantId(eventForInvitation, email);

            return new EmailInformation
            {
                CurrentEvent = eventForInvitation,
                OrganizerEmail = user.Email,
                OrganizerName = user.FirstName,
                ParticipantId = participantId,
                ParticipantEmail = email,
                ResponseUrl = CreateUrl("Response", "Response", eventForInvitation, participantId),
                RemainderDate = eventForInvitation.ReminderDate.GetValueOrDefault(),
                ListDate = eventForInvitation.ListDate.GetValueOrDefault(),
                EventDetailsUrl = CreateUrl("Details", "Events", eventForInvitation, 0),
                EmailSubject = " Invitation"
            };
        }

        private void SaveParticipantInDb(string email, Event eventForInvitation)
        {
            var invitedParticipant = _service.CreateParticipant(email);
            eventForInvitation.Participants.Add(invitedParticipant);
            _db.SaveChanges();
        }

        private static int GetParticipantId(Event eventForInvitation, string email)
        {
            return eventForInvitation.Participants
                                     .Where(p => p.Email == email)
                                     .Select(p => p.Id)
                                     .FirstOrDefault();
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