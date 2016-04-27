using System;
using Hangfire;
using System.Linq;
using System.Collections.Generic;
using SchedulerWebApp.Models.DBContext;
using SchedulerWebApp.Models.PostalEmail;

namespace SchedulerWebApp.Models.HangfireJobs
{
    public static class JobManager
    {
        static readonly SchedulerDbContext Db = new SchedulerDbContext();

        public static void ScheduleParticipantListEmail(EmailInformation emailInfo, DateTime listDate)
        {
            const string jobDescription = "Send list";
            var currentEvent = emailInfo.CurrentEvent;

            RemoveOldListJob(currentEvent, jobDescription);

            var listJobId = BackgroundJob.Schedule(() => PostalEmailManager.SendListEmail(emailInfo, new ParticipantListEmail()), listDate);
            AddJobsIntoEvent(listJobId, currentEvent.Id, jobDescription);
        }

        public static void ScheduleRemainderEmail(List<EmailInformation> emails, DateTime remainderDate)
        {
            var remainderJobId = BackgroundJob.Schedule(() => PostalEmailManager.SendRemainder(emails, new RemainderEmail()), remainderDate);
            AddJobsIntoEvent(remainderJobId, emails.First().CurrentEvent.Id, "Send Remainder");
        }

        public static void AddJobsIntoEvent(string jobId, int eventId, string jobDescription)
        {
            var scheduledJob = new ScheduledJob
            {
                EventId = eventId,
                JobId = jobId,
                JobDescription = jobDescription
            };
            Db.ScheduledJobs.Add(scheduledJob);
            Db.SaveChanges();
        }

        public static void RemoveScheduledJobs(Event eventToEdit)
        {
            var scheduledJobs = Db.ScheduledJobs
                                  .Where(sj => sj.EventId == eventToEdit.Id)
                                  .ToList();

            foreach (var job in scheduledJobs)
            {
                BackgroundJob.Delete(job.JobId);
                Db.ScheduledJobs.Remove(job);
                Db.SaveChanges();
            }
        }

        public static void RemoveOldListJob(Event currentEvent, string jobDescription)
        {
            var oldListJob = Db.ScheduledJobs.FirstOrDefault(sj => (sj.EventId == currentEvent.Id) &&
                                                                (sj.JobDescription == jobDescription));
            if (oldListJob == null) return;
            BackgroundJob.Delete(oldListJob.JobId);
            Db.ScheduledJobs.Remove(oldListJob);
            Db.SaveChanges();
        }
    }
}