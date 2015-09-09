using System.Collections.Generic;

namespace SchedulerWebApp.Models.ViewModels
{
    public class UnsavedContactViewModel
    {
        public int EventId { get; set; }
        public List<Contact> Contacts { get; set; }
    }
}