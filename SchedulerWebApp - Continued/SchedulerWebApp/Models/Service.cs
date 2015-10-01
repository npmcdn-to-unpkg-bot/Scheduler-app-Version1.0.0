using System;

namespace SchedulerWebApp.Models
{
    public static class Service
    {
        public static DateTime GetRemanderDate(Event eventToEdit)
        {
            var date = eventToEdit.ReminderDate;
            var remanderDate = date.Add(TimeSpan.FromHours(08));
            return remanderDate;
        }

        public static DateTime GetListDate(Event eventToEdit)
        {
            var date = eventToEdit.ListDate;
            var listDate = date.Add(TimeSpan.FromHours(08));
            return listDate;
        }
    }
}