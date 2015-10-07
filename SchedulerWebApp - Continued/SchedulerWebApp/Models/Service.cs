using System;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using SchedulerWebApp.Models.PostalEmail;

namespace SchedulerWebApp.Models
{
    public static class Service
    {
        public static DateTime GetRemanderDate(Event eventToEdit)
        {
            var remanderDate = eventToEdit.ReminderDate;
            //var remainderDateUtc = DateTime.SpecifyKind(remanderDate, DateTimeKind.Utc);
            return remanderDate;
        }

        public static DateTime GetListDate(Event eventToEdit)
        {
            var listDate = eventToEdit.ListDate;
            //var listDateUtc = DateTime.SpecifyKind(listDate, DateTimeKind.Utc);
            return listDate;
        }

        public static Attachment CreateAttachment(EmailInformation emailInformation)
        {
            var eventToSend = emailInformation.CurrentEvent;

            var startDate = eventToSend.StartDate.ToUniversalTime();
            var endDate = startDate.Add(TimeSpan.FromHours(4));

            //Build attachment
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("BEGIN:VCALENDAR");
            stringBuilder.AppendLine("PRODID:-//Invitation");
            stringBuilder.AppendLine("VERSION:2.0");
            stringBuilder.AppendLine("METHOD:REQUEST");
            stringBuilder.AppendLine("BEGIN:VEVENT");
            stringBuilder.AppendLine(string.Format("DTSTART:{0:yyyyMMddTHHmmssZ}", startDate));     //Verify if the time is local time
            stringBuilder.AppendLine(string.Format("DTSTAMP:{0:yyyyMMddTHHmmssZ}", DateTime.UtcNow));
            stringBuilder.AppendLine(string.Format("DTEND:{0:yyyyMMddTHHmmssZ}", endDate));
            stringBuilder.AppendLine(string.Format("LOCATION: {0}", eventToSend.Location));
            stringBuilder.AppendLine(string.Format("UID:{0}", Guid.NewGuid()));
            stringBuilder.AppendLine(string.Format("DESCRIPTION:{0}", eventToSend.Description));
            stringBuilder.AppendLine(string.Format("X-ALT-DESC;FMTTYPE=text/html:{0}", eventToSend.Description));
            stringBuilder.AppendLine(string.Format("SUMMARY:{0}", eventToSend.Description));
            stringBuilder.AppendLine(string.Format("ORGANIZER:MAILTO:{0}", emailInformation.OrganizerEmail));
            //stringBuilder.AppendLine(string.Format("ATTENDEE;CN=\"{0}\";RSVP=TRUE:mailto:{1}", email.SenderLastName, email.SenderEmail));
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
            contentType.Parameters.Add("name", string.Format("{0}'s_Invitation.ics", eventToSend.Title));

            //create attachment
            var attachment = new Attachment(stream, contentType);

            //return Attachment
            return attachment;

        }
    }
}