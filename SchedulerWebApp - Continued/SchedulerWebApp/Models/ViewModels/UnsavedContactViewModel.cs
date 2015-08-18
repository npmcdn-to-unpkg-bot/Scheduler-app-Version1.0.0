using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchedulerWebApp.Models.ViewModels
{
    public class UnsavedContactViewModel
    {
        public int EventId { get; set; }

        public List<Contact> Contacts { get; set; }
    }
}