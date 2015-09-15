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
        static readonly List<string> JobIds = new List<string>();
        static readonly SchedulerDbContext Db = new SchedulerDbContext();

        public static void ScheduleParticipantListEmail(EmailInformation emailInfo)
        {
            var listJobId = BackgroundJob.Schedule(
                () => PostalEmailManager.SendListEmail(emailInfo, new ParticipantListEmail()),
                TimeSpan.FromMinutes(8));
            JobIds.Add(listJobId);
        }

        public static void ScheduleRemainderEmail(EmailInformation emailInfo)
        {
            var remainderJobId = BackgroundJob.Schedule(() => PostalEmailManager.SendRemainder(emailInfo, new RemainderEmail()),
                TimeSpan.FromMinutes(5));

            JobIds.Add(remainderJobId);
        }

        public static void AddJobsIntoEvent(int eventID)
        {
            foreach (var jobId in JobIds)
            {
                var scheduledJob = new ScheduledJob
                {
                    EventId = eventID,
                    JobId = jobId
                };
                Db.ScheduledJobs.Add(scheduledJob);
                Db.SaveChanges();
            }
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
    }
}