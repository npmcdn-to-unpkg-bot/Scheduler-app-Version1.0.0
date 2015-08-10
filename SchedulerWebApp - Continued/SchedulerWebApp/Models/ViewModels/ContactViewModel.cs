using System.ComponentModel.DataAnnotations;

namespace SchedulerWebApp.Models.ViewModels
{
    public class ContactViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50,MinimumLength = 3)]
        public string SenderFistName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, MinimumLength = 3)]
        public string SenderLastName { get; set; }

        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Your email")]
        public string SenderEmail { get; set; }

        [Required]
        [Display(Name = "Subject")]
        public string EmailSubject { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Message")]
        public string EmailBody { get; set; }
    }
}