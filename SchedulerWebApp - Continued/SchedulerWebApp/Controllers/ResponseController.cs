﻿using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SchedulerWebApp.Models;
using SchedulerWebApp.Models.DBContext;
using SchedulerWebApp.Models.ViewModels;

namespace SchedulerWebApp.Controllers
{
    [AllowAnonymous]
    public class ResponseController : Controller
    {
        private readonly SchedulerDbContext _db = new SchedulerDbContext();

        public ResponseController()
        {
        }

        public ResponseController(SchedulerDbContext context)
        {
            _db = context;
        }

        public ActionResult Response(int? id, int pId)
        {
            if (id == null)
            {
                return View("Error");
            }
            var eventToAttend = _db.Events.Find(id);
            var participant = eventToAttend.Participants.FirstOrDefault(p => p.Id == pId);

            if (participant == null)
            {
                return HttpNotFound();
            }

            if (!CheckIfEventHasPassed(eventToAttend))
            {
                return View("_CantRespond", eventToAttend);
            }
            var model = new ResponseViewModel
                        {
                            Availability = participant.Availability,
                            Responce = participant.Responce,
                            ParticipantEmail = participant.Email,
                            ParticipantId = participant.Id,
                            EventId = (int)id,
                            Comments = participant.Comments,
                            EventsTitle = eventToAttend.Title,
                            EventsLocation = eventToAttend.Location,
                            EventDate = eventToAttend.StartDate.GetValueOrDefault()
                        };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Response([Bind(Include =
            "ParticipantId, EventId, ReceiverEmail, Availability, Comments, EventsTitle, EventsLocation")] ResponseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var eventId = model.EventId;
            var @event = _db.Events.Find(eventId);

            try
            {
                var participant = @event.Participants.SingleOrDefault(p => p.Id == model.ParticipantId);
                if (participant != null)
                {
                    participant.Responce = true;
                    participant.Availability = model.Availability;
                    participant.Comments = model.Comments;
                    _db.Participants.AddOrUpdate(participant);
                }
                _db.SaveChanges();
            }
            catch (Exception)
            {
                return View(model);
            }

            return RedirectToAction("Details", new { id = eventId });
        }

        private static bool CheckIfEventHasPassed(Event @event)
        {
            var dateToday = DateTime.UtcNow.Date;
            var listDate = @event.ListDate.GetValueOrDefault().Date;
            bool canStillRespond = dateToday <= listDate;
            return canStillRespond;
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            var @event = _db.Events.Find(id);
            return View(@event);
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