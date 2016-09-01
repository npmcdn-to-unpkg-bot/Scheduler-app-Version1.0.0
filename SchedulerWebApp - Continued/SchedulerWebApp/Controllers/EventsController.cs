using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Elmah;
using SchedulerWebApp.Models;
using SchedulerWebApp.Models.DBContext;
using SchedulerWebApp.Models.HangfireJobs;
using SchedulerWebApp.Models.PostalEmail;
using SchedulerWebApp.Models.ValidationAttributes;

namespace SchedulerWebApp.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly SchedulerDbContext _db = new SchedulerDbContext();
        private readonly Service _service = new Service();

        public EventsController()
        {
        }

        public EventsController(SchedulerDbContext context, Service service)
        {
            _db = context;
            _service = service;
        }

        private string UserId
        {
            get { return _service.GetUserId(); }
        }

        private bool IsAdmin
        {
            get { return _service.UserIsAdmin(); }
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
            if (IsAdmin)
            {
                var allEvents = _db.Events
                                   .ToList()
                                   .OrderBy(e => e.StartDate);

                return !allEvents.Any() ? PartialView("_NoEvent") : PartialView("_EventList", allEvents);
            }

            var userEvents = _service.GetUserEvents(UserId)
                            .OrderBy(e => e.StartDate);

            return !userEvents.Any() ? PartialView("_NoEvent") : PartialView("_EventList", userEvents);
        }

        #endregion

        #region Upcoming Events

        [ChildActionOnly]
        public ActionResult UpcomingEvents()
        {
            var dateToday = DateTime.UtcNow.ToLocalTime();

            if (IsAdmin)
            {
                var allEvents = _db.Events.Where(e => e.StartDate >= dateToday)
                                          .ToList()
                                          .OrderBy(e => e.StartDate);

                return !allEvents.Any() ? PartialView("_NoEvent") : PartialView("_EventList", allEvents);
            }
            var userEvents = _service.GetUserEvents(UserId).Where(e => e.StartDate >= dateToday)
                                        .ToList()
                                        .OrderBy(e => e.StartDate);

            return !userEvents.Any() ? PartialView("_NoEvent") : PartialView("_EventList", userEvents);
        }
        #endregion

        #region Passed Events

        [ChildActionOnly]
        public ActionResult PreviousEvents()
        {
            var dateToday = DateTime.UtcNow.ToLocalTime();

            if (IsAdmin)
            {
                var allEvents = _db.Events.Where(e => e.StartDate < dateToday)
                                          .ToList()
                                          .OrderBy(e => e.StartDate);

                return !allEvents.Any() ? PartialView("_NoEvent") : PartialView("_EventList", allEvents);
            }
            var userEvents = _service.GetUserEvents(UserId).Where(e => e.StartDate < dateToday)
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
                return View("Error");
            }

            if (IsAdmin)
            {
                var @event = _db.Events.Find(id);
                if (@event == null)
                {
                    return View("Error");
                }
                return View(@event);
            }
            var userEvent = _service.GetUserSpecificEvent(id);
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

            // ReSharper disable once PossibleInvalidOperationException
            ConvertDateTime.ToSwissTimezone(TimeZoneInfo.ConvertTimeToUtc((DateTime)@event.StartDate));
            /*
                 * Todo: the line below is only for debugging purposes:
                 *       Its to log current time to see if the application is using client or server machine
                 *       local time
                 */
            ErrorSignal.FromCurrentContext().Raise(new Exception(String.Format("The current Event Date(local swiss time) is: {0}.  - (From Events controller)", @event.StartDate)));

            _service.SaveEvent(@event);
            return RedirectToAction("SendEventsInvitation", "Invitation", new { id = @event.Id });
        }

        public ActionResult CopyEvent(int id)
        {
            var eventToCopy = _service.GetUserSpecificEvent(id);
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
            // ReSharper disable once PossibleInvalidOperationException
            ConvertDateTime.ToSwissTimezone(TimeZoneInfo.ConvertTimeToUtc((DateTime)@event.StartDate));
            /*
                 * Todo: the line below is only for debugging purposes:
                 *       Its to log current time to see if the application is using client or server machine
                 *       local time
                 */
            ErrorSignal.FromCurrentContext().Raise(new Exception(String.Format("The current Event Date(local swiss time) is: {0}.  - (From Events controller)", @event.StartDate)));

            _service.SaveEvent(@event);
            return RedirectToAction("SendEventsInvitation", "Invitation", new { id = @event.Id });
        }

        #endregion

        #region Edit Event

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var eventToEdit = _service.GetUserSpecificEvent(id);

            //if not the organizer
            if (eventToEdit == null)
            {
                return View("_NoAccess");
            }

            //check if event has not passed
            return !_service.EventHasNotPassed(eventToEdit) ? View("_LateToManage", eventToEdit) : View(eventToEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Location,Description,StartDate,EndDate,ReminderDate,ListDate,SchedulerUserId")] 
            Event eventToEdit, int? id)
        {
            var results = eventToEdit.StartDate.GetValueOrDefault().CompareTo(eventToEdit.ListDate.GetValueOrDefault());

            if (results < 0)
            {
                ModelState.AddModelError("dateError", "Enter date before Starting date");
            }

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

            var user = _service.GetUser();

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


            //Schedule remainders only when there is a remainder date
            if (eventToEdit.ReminderDate != null)
            {
                JobManager.ScheduleRemainderEmail(emails, remanderDate);
            }
            //Schedule new emails for edited Job
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
                return View("Error");
            }

            if (_service.UserIsAdmin())
            {
                var @event = _db.Events.Find(id);
                return View(@event);
            }
            var userEvent = _service.GetUserSpecificEvent(id);

            return userEvent == null ? View("_NoAccess") : View(userEvent);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var user = _service.GetUser();
            var @event = _db.Events.Include(ev => ev.Participants)
                                          .FirstOrDefault(e => (e.Id == id) && (e.SchedulerUserId == UserId));


            //if event hasn't occured notify users of cancellation
            if (_service.EventHasNotPassed(@event))
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