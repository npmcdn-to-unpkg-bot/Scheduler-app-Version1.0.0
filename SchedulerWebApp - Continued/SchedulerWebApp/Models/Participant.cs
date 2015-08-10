using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchedulerWebApp.Models
{
    public class Participant
    {
        public int Id { get; set; }
        public string Email { get; set; }

        [DataType(DataType.MultilineText)]
        public string Comments { get; set; }

        public bool Availability { get; set; }
        public bool Responce { get; set; }

        public virtual List<Event> Events { get; set; }
    }
}