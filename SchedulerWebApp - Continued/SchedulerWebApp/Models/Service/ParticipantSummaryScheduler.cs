using System;
using Quartz;
using Quartz.Impl;

namespace SchedulerWebApp.Models.Service
{
    public class ParticipantSummaryScheduler
    {
        public void Start(int eventId, string organizerName, string userEmail, DateTime participantListDate)
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            //create job to be performed
            IJobDetail job = JobBuilder.Create<ParticipantSummaryJob>()
                                       .UsingJobData("eventId", eventId)
                                       .UsingJobData("organizerName", organizerName)
                                       .UsingJobData("userEmail", userEmail)
                                       .Build();

            //create trigger that will triger job
            ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
                                     .StartAt(participantListDate)
                                     .Build();

            //register job and its trigger in scheduler
            scheduler.ScheduleJob(job, trigger);
        }
    }
}