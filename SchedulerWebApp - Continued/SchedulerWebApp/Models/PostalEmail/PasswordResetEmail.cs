using Postal;

namespace SchedulerWebApp.Models.PostalEmail
{
    public class PasswordResetEmail : Email
    {
        public string ReceiverEmail { get; set; }
        public string ReceiverName { get; set; }
        public string AdminEmail { get; set; }
        public string EmailSubject { get; set; }
        public string PassWordRestLink { get; set; }
    }
}