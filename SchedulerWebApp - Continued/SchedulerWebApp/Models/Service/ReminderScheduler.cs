using System;
using Quartz;
using Quartz.Impl;

namespace SchedulerWebApp.Models.Service
{
    public class ReminderScheduler
    {
        public void Start(DateTime remindDate, int eventId, string senderName, string senderEmail)
        {
            //intialize scheduler ans start it
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            //create job to be perfomed
            IJobDetail job = JobBuilder.Create<ReminderJob>()
                                       .UsingJobData("eventId",eventId)
                                       .UsingJobData("senderName",senderName)
                                       .UsingJobData("senderEmail",senderEmail)
                                       .Build();

            //create trigger that will triger job
            ISimpleTrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
                                     .StartAt(remindDate)
                                     .Build();

            //register job and its trigger in scheduler
            scheduler.ScheduleJob(job, trigger);
        }
    }
}