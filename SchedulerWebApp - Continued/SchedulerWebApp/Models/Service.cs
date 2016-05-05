using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using SchedulerWebApp.Models.DBContext;
using SchedulerWebApp.Models.PostalEmail;

namespace SchedulerWebApp.Models
{
    public static class Service
    {
        private static SchedulerDbContext _db;

        public static SchedulerUser GetUser(string userid)
        {
            using (_db = new SchedulerDbContext())
            {
                return _db.Users.Find(userid);
            }
        }

        public static Event GetUserSpecificEvent(string userId, int? id)
        {
            using (_db = new SchedulerDbContext())
            {
                var userEvents = _db.Users.Find(userId).Events.Find(e => e.Id == id);
                return userEvents;
            }
        }

        public static DateTime GetRemanderDate(Event eventToEdit)
        {
            var remanderDate = eventToEdit.ReminderDate;
            return remanderDate.GetValueOrDefault();
        }

        public static DateTime GetListDate(Event eventToEdit)
        {
            var listDate = eventToEdit.ListDate;
            return listDate.GetValueOrDefault();
        }

        public static Attachment CreateAttachment(EmailInformation emailInformation)
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
            stringBuilder.AppendLine(String.Format("X-ALT-DESC;FMTTYPE=text/html:{0}", eventToSend.Description));
            stringBuilder.AppendLine(String.Format("SUMMARY:{0}", eventToSend.Description));
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

        public static string RemoveBrackets(string email)
        {
            if (email.Contains("["))
            {
                email = email.Split('[', ']')[1];
            }
            return email;
        }

        public static DateTime? SetCorectDate(DateTime? dateToset)
        {
            DateTime? corectDate = null;

            if (dateToset != null)
            {
                corectDate = dateToset;
            }
            return corectDate;
        }

        public static bool EventHasNotPassed(Event eventForInvitation)
        {
            var todaysDate = DateTime.UtcNow.ToLocalTime();
            var compareDates = eventForInvitation.StartDate.GetValueOrDefault().CompareTo(todaysDate);

            bool notPassed = compareDates >= 0;

            return notPassed;
        }

        public static Participant CreateParticipant(string email)
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