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

        public static void ScheduleParticipantListEmail(EmailInformation emailInfo, DateTime listDate)
        {
            var listJobId = BackgroundJob.Schedule(() => PostalEmailManager.SendListEmail(emailInfo, new ParticipantListEmail()), listDate);
            JobIds.Add(listJobId);
        }
        
        public static void ScheduleRemainderEmail(List<EmailInformation> emails, DateTime remainderDate)
        {
            var remainderJobId = BackgroundJob.Schedule(() => PostalEmailManager.SendRemainder(emails, new RemainderEmail()), remainderDate);
            JobIds.Add(remainderJobId);
        }

        public static void AddJobsIntoEvent(int eventId)
        {
            foreach (var jobId in JobIds)
            {
                var scheduledJob = new ScheduledJob
                {
                    EventId = eventId,
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