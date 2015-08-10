using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using SchedulerWebApp.Models;
using SchedulerWebApp.Models.DBContext;
using SchedulerWebApp.Models.Service;

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
                return View(@event);
            }
            var userEvent = GetUserEvents().Find(e => e.Id == id);
            return userEvent == null ? View("_NoAccess") : View(userEvent);
        }

        #endregion

        #region Create Event

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "Title,Location,Description,StartDate,EndDate,ReminderDate,ListDate")] Event @event)
        {
            if (!ModelState.IsValid)
            {
                return View(@event);
            }
            GetUserEvents().Add(@event);
            _db.SaveChanges();
            return RedirectToAction("SendEventsInvitation", "Invitation", new { id = @event.Id });
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
        public ActionResult Edit(
            [Bind(Include = "Id,Title,Location,Description,StartDate,EndDate,ReminderDate,ListDate,SchedulerUserId")] Event
                eventToEdit, int? id)
        {

            if (ModelState.IsValid)
            {
                _db.Entry(eventToEdit).State = EntityState.Modified;
                _db.SaveChanges();

                //get user 
                var userId = User.Identity.GetUserId();
                var user = _db.Users.Find(userId);

                //Notify participants of the changes
                var emailDelivery = new EmailDelivery();
                emailDelivery.SendInfoEmail(eventToEdit.Id, user.FirstName, user.UserName, "has been modified", "EditedEventInfo");

                return RedirectToAction("Index");
            }
            return View(eventToEdit);
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
            var @event = GetUserEvents().Find(e => e.Id == id);

            //if event hasn't occured notify users of cancellation
            if (EventHasNotOccured(@event))
            {
                //get user 
                var userId = User.Identity.GetUserId();
                var user = _db.Users.Find(userId);

                //Notify participant of the cancellation
                var emailDelivery = new EmailDelivery();
                emailDelivery.SendInfoEmail(id, user.FirstName, user.UserName, "have been cancelled", "DeletedEventInfo");

            }

            _db.Events.Remove(@event);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        #endregion

        public List<Event> GetUserEvents()
        {
            var currentUser = User.Identity.GetUserId();
            var userEvents = _db.Users.Single(u => u.Id==currentUser).Events;
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
            var eventEndDate = @event.EndDate.Date;
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