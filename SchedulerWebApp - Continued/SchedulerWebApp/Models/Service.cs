using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;
using Microsoft.AspNet.Identity;
using SchedulerWebApp.Models.DBContext;
using SchedulerWebApp.Models.PostalEmail;
using SchedulerWebApp.Models.ValidationAttributes;

namespace SchedulerWebApp.Models
{
    public class Service
    {
        private SchedulerDbContext _db;

        private string UserId
        {
            get { return GetUserId(); }
        }

        #region User methods

        public string GetUserId()
        {
            return HttpContext.Current.User.Identity.GetUserId();
        }

        public SchedulerUser GetUser()
        {
            using (_db = new SchedulerDbContext())
            {
                var user = _db.Users.Include(u => u.Events.Select(e => e.Participants))
                                .Include(u => u.Contacts)
                                .FirstOrDefault(u => u.Id == UserId);
                return user;
            }
        }

        public bool UserIsAdmin()
        {
            var isAdmin = HttpContext.Current.User.IsInRole("Admin");
            return isAdmin;
        }

        #endregion

        #region Date mothods

        public static DateTime? SetCorectDate(DateTime? dateToset)
        {
            DateTime? corectDate = null;

            if (dateToset != null)
            {
                corectDate = dateToset;
            }
            return corectDate;
        }

        public static DateTime GetListDate(Event eventToEdit)
        {
            var listDate = eventToEdit.ListDate;
            return listDate.GetValueOrDefault();
        }

        public static DateTime GetRemanderDate(Event eventToEdit)
        {
            // ReSharper disable once PossibleInvalidOperationException
            var remanderDateUtc = TimeZoneInfo.ConvertTimeToUtc((DateTime) eventToEdit.ReminderDate);

            var remainderDate = ConvertDateTime.ToSwissTimezone(remanderDateUtc);

            return remainderDate;
        }

        #endregion

        #region Event Related methods

        public List<Event> GetUserEvents(string userId)
        {
            var userEvents = GetUser().Events;
            return userEvents;
        }

        public Event GetUserSpecificEvent(int? id)
        {
            using (_db = new SchedulerDbContext())
            {
                var userEvents = _db.Events.Include(ev => ev.Participants)
                                          .FirstOrDefault(e => (e.Id == id) && (e.SchedulerUserId == UserId));

                /*_db.Users.Find(UserId).Events.Find(e => e.Id == id);*/
                return userEvents;
            }
        }

        public void SaveEvent(Event @event)
        {
            using (_db = new SchedulerDbContext())
            {
                _db.Users.Find(UserId).Events.Add(@event);
                _db.SaveChanges();
            }
        }

        public bool EventHasNotPassed(Event eventForInvitation)
        {
            var todaysDate = ConvertDateTime.ToSwissTimezone(DateTime.UtcNow);
            var compareDates = eventForInvitation.StartDate.GetValueOrDefault().CompareTo(todaysDate);

            bool notPassed = compareDates >= 0;
            return notPassed;
        }

        #endregion

        public Attachment CreateAttachment(EmailInformation emailInformation)
        {
            var eventToSend = emailInformation.CurrentEvent;

            var startDate = eventToSend.StartDate.GetValueOrDefault().ToUniversalTime();
            var endDate = startDate.Add(TimeSpan.FromHours(4));

            //Build attachment
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("BEGIN:VCALENDAR");
            stringBuilder.AppendLine("PRODID:-//Invitation");
            stringBuilder.AppendLine("VERSION:2.0");
            stringBuilder.AppendLine("METHOD:REQUEST");

            stringBuilder.AppendLine("BEGIN:VEVENT");
            stringBuilder.AppendLine("SUMMARY;LANGUAGE=en-us:" + eventToSend.Title);
            stringBuilder.AppendLine(String.Format("DTSTART:{0:yyyyMMddTHHmmssZ}", startDate));     //Verify if the time is local time
            stringBuilder.AppendLine(String.Format("DTSTAMP:{0:yyyyMMddTHHmmssZ}", DateTime.UtcNow));
            stringBuilder.AppendLine(String.Format("DTEND:{0:yyyyMMddTHHmmssZ}", endDate));
            stringBuilder.AppendLine(String.Format("LOCATION: {0}", eventToSend.Location));
            stringBuilder.AppendLine(String.Format("UID:{0}", Guid.NewGuid()));
            stringBuilder.AppendLine(String.Format("DESCRIPTION:{0}", eventToSend.Description));
            stringBuilder.AppendLine(String.Format("X-ALT-DESC;FMTTYPE=text/html:{0}", eventToSend.Title));
            stringBuilder.AppendLine(String.Format("SUMMARY:{0}", eventToSend.Title));
            stringBuilder.AppendLine(String.Format("ORGANIZER:MAILTO:{0}", emailInformation.OrganizerEmail));
            //stringBuilder.AppendLine(string.Format("ATTENDEE;CN=\"{0}\";RSVP=TRUE:mailto:{1}", email.SenderLastName, email.From));
            stringBuilder.AppendLine("BEGIN:VALARM");
            stringBuilder.AppendLine("TRIGGER:-PT15M");
            stringBuilder.AppendLine("ACTION:DISPLAY");
            stringBuilder.AppendLine("DESCRIPTION:Reminder");
            stringBuilder.AppendLine("END:VALARM");
            stringBuilder.AppendLine("END:VEVENT");
            stringBuilder.AppendLine("END:VCALENDAR");

            //store it in Memory stream
            var bytes = Encoding.ASCII.GetBytes(stringBuilder.ToString());
            MemoryStream stream = null;

            using (stream)
            {
                stream = new MemoryStream(bytes);
            }

            var contentType = new ContentType("text/calender");
            contentType.Parameters.Add("method", "REQUEST");
            contentType.Parameters.Add("name", String.Format("{0}'s_Invitation.ics", eventToSend.Title));

            //create attachment
            var attachment = new Attachment(stream, contentType);

            //return Attachment
            return attachment;

        }

        public string RemoveBrackets(string email)
        {
            if (email.Contains("["))
            {
                email = email.Split('[', ']')[1];
            }
            return email;
        }

        public Participant CreateParticipant(string email)
        {
            return new Participant
            {
                Email = email,
                Responce = false,
                Availability = false
            };
        }

    }
}