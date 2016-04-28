using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using SchedulerWebApp.Models;
using SchedulerWebApp.Models.DBContext;
using SchedulerWebApp.Models.HangfireJobs;
using SchedulerWebApp.Models.PostalEmail;

namespace SchedulerWebApp.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly SchedulerDbContext _db = new SchedulerDbContext();

        public EventsController()
        {
        }

        public EventsController(SchedulerDbContext context)
        {
            _db = context;
        }

        #region Return Events view

        public ActionResult Index()
        {
            return View();
        }

        #endregion

        #region AllEvents

        [ChildActionOnly]
        public ActionResult AllEvents()
        {
            if (UserIsAdmin())
            {
                var allEvents = _db.Events
                                   .ToList()
                                   .OrderBy(e => e.StartDate);

                return !allEvents.Any() ? PartialView("_NoEvent") : PartialView("_EventList", allEvents);
            }

            var userEvents = GetUserEvents()
                            .OrderBy(e => e.StartDate);

            return !userEvents.Any() ? PartialView("_NoEvent") : PartialView("_EventList", userEvents);
        }

        #endregion

        #region Upcoming Events

        [ChildActionOnly]
        public ActionResult UpcomingEvents()
        {
            var dateToday = DateTime.UtcNow.Date;

            if (UserIsAdmin())
            {
                var allEvents = _db.Events.Where(e => e.StartDate >= dateToday)
                                          .ToList()
                                          .OrderBy(e => e.StartDate);

                return !allEvents.Any() ? PartialView("_NoEvent") : PartialView("_EventList", allEvents);
            }
            var userEvents = GetUserEvents().Where(e => e.StartDate >= dateToday)
                                        .ToList()
                                        .OrderBy(e => e.StartDate);

            return !userEvents.Any() ? PartialView("_NoEvent") : PartialView("_EventList", userEvents);
        }

        #endregion

        #region Passed Events

        [ChildActionOnly]
        public ActionResult PreviousEvents()
        {
            var dateToday = DateTime.UtcNow.Date;

            if (UserIsAdmin())
            {
                var allEvents = _db.Events.Where(e => e.StartDate < dateToday)
                                          .ToList()
                                          .OrderBy(e => e.StartDate);

                return !allEvents.Any() ? PartialView("_NoEvent") : PartialView("_EventList", allEvents);
            }
            var userEvents = GetUserEvents().Where(e => e.StartDate < dateToday)
                                            .ToList()
                                            .OrderBy(e => e.StartDate);

            return !userEvents.Any() ? PartialView("_NoEvent") : PartialView("_EventList", userEvents);
        }
        #endregion

        #region Single Event details

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (UserIsAdmin())
            {
                var @event = _db.Events.Find(id);
                if (@event == null)
                {
                    return View("Error");
                }
                return View(@event);
            }
            var userEvent = GetUserEvents().Find(e => e.Id == id);
            return userEvent == null ? View("_NoAccess") : View(userEvent);
        }

        #endregion

        #region Create/Copy Event

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "Title,Location,Description,StartDate,EndDate")] Event @event)
        {
            if (!ModelState.IsValid)
            {
                return View(@event);
            }
            SaveEvent(@event);
            return RedirectToAction("SendEventsInvitation", "Invitation", new { id = @event.Id });
        }

        public ActionResult CopyEvent(int id)
        {
            var eventToCopy = GetUserEvents().Find(e => e.Id == id);
            var copiedEvent = eventToCopy;
            copiedEvent.Title = eventToCopy.Title + "-Copy";
            return View("Create", copiedEvent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CopyEvent(
            [Bind(Include = "Title,Location,Description,StartDate,EndDate")] Event @event)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", @event);
            }

            SaveEvent(@event);

            return RedirectToAction("SendEventsInvitation", "Invitation", new { id = @event.Id });
        }

        private void SaveEvent(Event @event)
        {
            GetUserEvents().Add(@event);
            _db.SaveChanges();
        }

        #endregion

        #region Edit Event

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var eventToEdit = GetUserEvents().FirstOrDefault(e => e.Id == id);

            //if not the organizer
            if (eventToEdit == null)
            {
                return View("_NoAccess");
            }

            //check if event has not passed
            return !EventHasNotOccured(eventToEdit) ? View("_LateToManage", eventToEdit) : View(eventToEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Location,Description,StartDate,EndDate,ReminderDate,ListDate,SchedulerUserId")] 
            Event eventToEdit, int? id)
        {
            if (!ModelState.IsValid)
            {
                return View(eventToEdit);
            }

            _db.Entry(eventToEdit).State = EntityState.Modified;
            _db.SaveChanges();

            var remanderDate = Service.GetRemanderDate(eventToEdit);
            var listDate = Service.GetListDate(eventToEdit);

            //Remove scheduled jobs of this event
            JobManager.RemoveScheduledJobs(eventToEdit);

            #region Send update emails to participants
            //Get Participants
            var currentEvent = _db.Events
                                  .Where(e => e.Id == eventToEdit.Id)
                                  .Include(e => e.Participants)
                                  .FirstOrDefault();

            if (currentEvent == null)
            {
                return View("Error");
            }

            var participants = currentEvent.Participants.ToList();

            var user = GetUser();

            EmailInformation emailInfo = null;
            var emails = new List<EmailInformation>();

            //There is no participants
            if (participants.Count == 0)
            {
                return RedirectToAction("Index");
            }

            foreach (var participant in participants)
            {
                var participantId = participant.Id;
                var detailsUrl = Url.Action("Details", "Response",
                    new RouteValueDictionary(new { id = currentEvent.Id }), "https");
                var responseUrl = Url.Action("Response", "Response",
                    new RouteValueDictionary(new { id = currentEvent.Id, pId = participantId }), "https");

                emailInfo = new EmailInformation
                            {
                                CurrentEvent = currentEvent,
                                OrganizerName = user.FirstName,
                                OrganizerEmail = user.UserName,
                                ParticipantId = participantId,
                                ParticipantEmail = participant.Email,
                                EmailSubject = " changes.",
                                ResponseUrl = responseUrl,
                                EventDetailsUrl = detailsUrl
                            };

                emails.Add(emailInfo);

                //Notify Participant using postal
                PostalEmailManager.SendEmail(emailInfo, new EmailInfoChangeEmail());
            }
            #endregion

            //Schedule new emails for edited Job
            JobManager.ScheduleRemainderEmail(emails, remanderDate);
            JobManager.ScheduleParticipantListEmail(emailInfo, listDate);
            //JobManager.AddJobsIntoEvent(eventToEdit.Id,"Send List");

            return RedirectToAction("Index");
        }

        #endregion

        #region Delete Event

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (UserIsAdmin())
            {
                var @event = _db.Events.Find(id);
                return View(@event);
            }
            var userEvent = GetUserEvents().FirstOrDefault(e => e.Id == id);

            return userEvent == null ? View("_NoAccess") : View(userEvent);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var user = GetUser();
            var @event = GetUserEvents().Find(e => e.Id == id);


            //if event hasn't occured notify users of cancellation
            if (EventHasNotOccured(@event))
            {
                var currentEvent = _db.Events.Where(e => e.Id == @event.Id)
                                    .Include(e => e.Participants).FirstOrDefault();

                if (currentEvent == null)
                {
                    return View("error");
                }
                var participants = currentEvent.Participants.ToList();


                foreach (var participant in participants)
                {
                    var emailInfo = new EmailInformation
                                                 {
                                                     CurrentEvent = @event,
                                                     OrganizerEmail = user.UserName,
                                                     OrganizerName = user.FirstName,
                                                     EmailSubject = " cancellation",
                                                     ParticipantEmail = participant.Email,
                                                     ParticipantId = participant.Id
                                                 };
                    PostalEmailManager.SendEmail(emailInfo, new CancellationEmail());
                }

                //Remove Scheduled emails for deleted Event
                JobManager.RemoveScheduledJobs(currentEvent);
            }

            _db.Events.Remove(@event);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        #endregion

        private SchedulerUser GetUser()
        {
            var userId = User.Identity.GetUserId();
            var user = _db.Users.Find(userId);
            return user;
        }

        public List<Event> GetUserEvents()
        {
            var currentUser = GetUser().Id;
            var userEvents = _db.Users.Single(u => u.Id == currentUser).Events;
            return userEvents;
        }

        private bool UserIsAdmin()
        {
            var isAdmin = User.IsInRole("Admin");
            return isAdmin;
        }

        private bool EventHasNotOccured(Event @event)
        {
            var todayDate = DateTime.UtcNow.Date;
            var eventEndDate = @event.StartDate.GetValueOrDefault().Date;
            var hasNotOccured = todayDate <= eventEndDate;
            return hasNotOccured;
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