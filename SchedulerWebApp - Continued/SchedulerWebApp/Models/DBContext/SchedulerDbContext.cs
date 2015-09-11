using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace SchedulerWebApp.Models.DBContext
{
    public class SchedulerDbContext : IdentityDbContext<SchedulerUser>
    {
        public SchedulerDbContext()
            : base("DefaultConnection", false)
        {
        }


        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<Participant> Participants { get; set; }
        public  virtual DbSet<Contact> Contacts { get; set; }
        public virtual DbSet<ScheduledJob> ScheduledJobs { get; set; }

        public static SchedulerDbContext Create()
        {
            return new SchedulerDbContext();
        }
    }
}