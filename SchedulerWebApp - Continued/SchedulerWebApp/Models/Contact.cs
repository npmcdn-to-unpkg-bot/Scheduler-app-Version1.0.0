using System.ComponentModel.DataAnnotations;
using SchedulerWebApp.Models.Validation;

namespace SchedulerWebApp.Models
{
    public class Contact
    {
        [ScaffoldColumn(false)]
        public int ContactId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [ValidEmail]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public virtual string SchedulerUserId { get; set; }
    }
}