using System.ComponentModel.DataAnnotations;
using SchedulerWebApp.Models.ValidationAttributes;

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
        [RegularExpression(@"^([0][2-7]{1}[1-9]{1}|(\+[4][1])?[ ]?[7][1-9]{1})?[ ]?(\d{3})?[ ]?(\d{2})?[ ]?(\d{2})$", ErrorMessage = "Entered mobile is not valid.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public virtual string SchedulerUserId { get; set; }
    }
}