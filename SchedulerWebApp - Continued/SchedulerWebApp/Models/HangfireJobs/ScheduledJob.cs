namespace SchedulerWebApp.Models.HangfireJobs
{
    public class ScheduledJob
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string JobId { get; set; }
    }
}