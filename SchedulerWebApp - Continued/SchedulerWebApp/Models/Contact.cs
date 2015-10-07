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
        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Entered mobile is not valid.")]
        [RegularExpression(@"^(?:\+\d{1,3}|0\d{1,3}|00\d{1,2})?(?:\s?\(\d+\))?(?:[-\/\s.]|\d)+$", ErrorMessage = "Entered mobile is not valid.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public virtual string SchedulerUserId { get; set; }
    }
}