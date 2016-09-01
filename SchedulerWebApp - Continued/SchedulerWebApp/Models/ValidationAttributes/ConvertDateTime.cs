using System;

namespace SchedulerWebApp.Models.ValidationAttributes
{
    public  static class ConvertDateTime
    {
        public static DateTime ToSwissTimezone(DateTime dateTimeNowUtc)
        {
            var swissTimezone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            var currentSwissTime = TimeZoneInfo.ConvertTime(dateTimeNowUtc, swissTimezone);
            return currentSwissTime;
        }
    }
}